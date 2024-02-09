using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Structure2D.Base.Utility;
using Unity.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Structure2D.Lighting
{
    public partial class BlockLighting
    {
        private partial class LightingThread
        {
            private EventWaitHandle _threadWaitHandle;
            private Coordinate[] _threadViewportCoordinates = new Coordinate[2];
            private byte _skyColor = 255;

            private int MapWidth;
            private int MapHeight;
            
            public bool IsLightingDataAvailable
            {
                get => (Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 1) == 1); 
        
                set
                {
                    if (value) Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 0);
        
                    else Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 0, 1);
                }
            }

            public bool IsActive { get; set; }

            // default is false, set 1 for true.
            private int _threadSafeBoolBackValue = 0;

            private Thread _lightingThread;
        
            private LightingInfo[][] _threadLightData;
            private LightingInfo[][] _axisReversedLightData;

            private NativeArray<Color32> _cellData;

            private Color32[] _lightingValues;
            
            private Viewport[] _actualViewport;

            private Dictionary<Coordinate, byte> _tempLights;
            
            public LightingThread()
            {
                _threadWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
            }
            
            public void Dispose()
            {
                _cellData.Dispose();
            }
            
            public void Start(int mapWidth, int mapHeight)
            {
                MapWidth = mapWidth;
                MapHeight = mapHeight;
                _lightingThread?.Abort();
                _actualViewport = new Viewport[1];
                
                _threadLightData = new LightingInfo[mapWidth][];
                _axisReversedLightData = new LightingInfo[mapHeight][];

                _tempLights = new Dictionary<Coordinate, byte>();
                
                _lightingValues = new Color32[mapWidth * mapHeight];

                if (_cellData.IsCreated)
                    _cellData.Dispose();
                
                _cellData = new NativeArray<Color32>(mapWidth * mapHeight, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                
                for (int y = 0; y < mapHeight; ++y)
                {
                    _axisReversedLightData[y] = new LightingInfo[mapWidth];
                }
        
                for (int x = 0; x < mapWidth; ++x)
                {
                    var array = new LightingInfo[mapHeight];

                    for (int y = 0; y < mapHeight; ++y)
                    {
                        var state = new LightingInfo();
                        array[y] = state;
                        _axisReversedLightData[y][x] = state;
                    }

                    _threadLightData[x] = array;
                }

                _lightingThread = new Thread(Update);
                _lightingThread.Start();
            }
        
            /// <summary>
            /// Returns the calculated values
            /// Only use this when IsLightingDataIsAvailable is true
            /// </summary>
            /// <returns></returns>
            public Color32[] GetValues()
            {
                return _lightingValues;
            }

            /// <summary>
            /// Sets this thread into the idle state
            /// </summary>
            public void Reset()
            {
                IsActive = false;
            }

            /// <summary>
            /// This triggers a new lighting calculation
            /// </summary>
            public void StartThreadCalculation(Dictionary<Coordinate, byte> tempLights)
            {
                IsActive = true;
                
                PrepareThread(tempLights);
                
                _threadWaitHandle.Set();
            }

        
            private void PrepareThread(Dictionary<Coordinate, byte> tempLights)
            {
                _skyColor = BlockLighting.SkyColor;
                var viewport = Viewport.CurrentViewport;

                _actualViewport[0] = viewport;
                
                viewport.BottomLeft.x -= MaxLightDistance;
                
                if (viewport.BottomLeft.x < 0)
                    viewport.BottomLeft.x = 0;
                
                viewport.Width += MaxLightDistance * 2;

                if (viewport.Width + viewport.BottomLeft.x >= CellMap.MapWidth)
                    viewport.Width = CellMap.MapWidth - viewport.BottomLeft.x;

                viewport.BottomLeft.y -= MaxLightDistance;

                if (viewport.BottomLeft.y < 0)
                    viewport.BottomLeft.y = 0;

                viewport.Height += MaxLightDistance * 2;

                if (viewport.Height + viewport.BottomLeft.y >= CellMap.MapHeight)
                    viewport.Height = CellMap.MapHeight - viewport.BottomLeft.y;
                
                _threadViewportCoordinates[0] = viewport.BottomLeft;
                _threadViewportCoordinates[1] = viewport.BottomLeft  + new Coordinate(viewport.Width, viewport.Height);
                
                PrepareCellValues(tempLights);
                
                /*
                for (int x = viewport.BottomLeft.x; x < viewport.BottomLeft.x + viewport.Width; ++x)
                {
                    for (int y = viewport.BottomLeft.y; y < viewport.BottomLeft.y + viewport.Height; ++y)
                    {
                        byte light = 0;
                        
                        var cellData = CellMap.GetCellUnsafe(x, y);

                        if (cellData.HasSunlight) 
                            light = SkyColor;
                        
                        _threadLightData[x][y].Value = light;
                        _threadLightData[x][y].BlockType = (byte)cellData.Block;
                        _threadLightData[x][y].BackgroundType = (byte) cellData.Background;
                    }
                }

                foreach (var tempLight in tempLights)
                {
                    var light = tempLight.Value;

                    if (light > _threadLightData[tempLight.Key.x][tempLight.Key.y].Value)
                        _threadLightData[tempLight.Key.x][tempLight.Key.y].Value = light;
                }*/
            }

            private void PrepareCellValues(Dictionary<Coordinate, byte> temporaryLights)
            {
                var shaderData = TerrainShaderData.FetchCellTextureData();
                
                NativeArray<Color32>.Copy(shaderData, _cellData);
                
                _tempLights.Clear();

                foreach (var temporaryLight in temporaryLights)
                {
                    _tempLights.Add(temporaryLight.Key, temporaryLight.Value);
                }
            }
            
            private void Update()
            {
                Stopwatch _watch = new Stopwatch();
                while (true)
                {
                    //We wait until we are told to generate lighting
                    if(!_threadWaitHandle.WaitOne())
                        continue;
            
                    _watch.Restart();
                    
                    _threadWaitHandle.Reset();

                    PrepareLightingValues();

                    CalculateLighting();
                    ApplyCellValues();
                    
                    //Let the main thread now that the lighting data is ready
                    IsLightingDataAvailable = true;
                    
                    DebugUtility.LogString(string.Format("Lighting thread took {0}ms to process", _watch.ElapsedMilliseconds));
                }
            }

            private void PrepareLightingValues()
            {
                var viewportStart = _threadViewportCoordinates[0];
                var viewportEnd = _threadViewportCoordinates[1];
                
                for (int x = viewportStart.x; x < viewportEnd.x; ++x)
                {
                    for (int y = viewportStart.y; y < viewportEnd.y; ++y)
                    {
                        byte light = 0;
                        
                        var cellInformation = _cellData[x + MapWidth * y];

                        var block = cellInformation.GetBlock();
                        var background = cellInformation.GetBackground();

                        int blockIndex = background > 1 ? 1 : background;

                        var blocksSunlight = block != 0 || (background != 0 && LightingMetaData.DoesBackgroundTypeBlockSunlight[blockIndex]);

                        if (!blocksSunlight)
                        {
                            if(_skyColor > light)    
                                light = _skyColor;
                        }

                        _threadLightData[x][y].Value = light;
                        _threadLightData[x][y].BlockType = block;
                        _threadLightData[x][y].BackgroundType = background;
                    }
                }

                foreach (var tempLight in _tempLights)
                {
                    if (tempLight.Value > _threadLightData[tempLight.Key.x][tempLight.Key.y].Value)
                        _threadLightData[tempLight.Key.x][tempLight.Key.y].Value = tempLight.Value;
                }
            }

            private void ApplyCellValues()
            {
                var viewportStart = _threadViewportCoordinates[0];
                var viewportEnd = _threadViewportCoordinates[1];
                
                for (int x = viewportStart.x; x < viewportEnd.x; ++x)
                {
                    for (int y = viewportStart.y; y < viewportEnd.y; ++y)
                    {
                        var color = new Color32(_threadLightData[x][y].Value, 0, 0, 0);

                        _lightingValues[MapWidth * y + x] = color;
                    }
                }
            }
            
            private void CalculateLighting()
            {
                var firstPassSwipeData = GetFirstSwipePassData();

                DoColors(firstPassSwipeData);
        
                var secondPassSwipeData = GetSecondSwipePassData();
        
                DoColors(secondPassSwipeData);
            }
            
            private void DoColors(LightSwipeData swipeData)
            {
                bool isFirstLoop = true;
                while (true)
                {
                    int innerLoopStart;
                    int innerLoopEnd;
                    int direction;

                    if (isFirstLoop)
                    {
                        direction = 1;
                        innerLoopStart = swipeData.InnerLoopStart ;
                        innerLoopEnd = swipeData.InnerLoopEnd;
                    }

                    else
                    {
                        direction = -1;
                        innerLoopStart = swipeData.InnerLoopEnd - 1;
                        innerLoopEnd = swipeData.InnerLoopStart - 1;
                    }

                    int outerLoopStart = swipeData.OuterLoopStart;
                    int outerLoopEnd = swipeData.OuterLoopEnd;
            
                    for (int i = outerLoopStart; i < outerLoopEnd; ++i)
                    {
                        LightingInfo[] row = swipeData.States[i];

                        byte currentLightValue = 0;

                        for (int j = innerLoopStart; j != innerLoopEnd; j += direction)
                        {
                            LightingInfo currentState = row[j];

                            if (currentState.Value > currentLightValue)
                            {
                                currentLightValue = currentState.Value;
                            }

                            else
                            {
                                currentState.Value = currentLightValue;
                            }

                            if (currentState.BackgroundType > 1)
                                currentState.BackgroundType = 1;
                            

                            var lightFallOff = currentState.BlockType != 0
                                ? LightingMetaData.BlocksLightBlockAmount[currentState.BlockType]
                                : LightingMetaData.BackgroundsLightBlockAmount[currentState.BackgroundType];
                            
                            if (currentLightValue < lightFallOff)
                                currentLightValue = 0;
                            else
                                currentLightValue -= lightFallOff;
                        }

                        swipeData.States[i] = row; 
                    }
                    
                    if(!isFirstLoop)
                        break;

                    isFirstLoop = false;
                }
            }

            private LightSwipeData GetSecondSwipePassData()
            {
                var swipeData = new LightSwipeData();
                swipeData.States = _axisReversedLightData; 
        
                //Bottom left cell of the viewport
                var startCoordinate = _threadViewportCoordinates[0];
        
                //Top right cell of the viewport
                var endCoordinate = _threadViewportCoordinates[1];

                swipeData.OuterLoopStart = startCoordinate.y;
                swipeData.OuterLoopEnd = endCoordinate.y;

                swipeData.InnerLoopStart = startCoordinate.x;
                swipeData.InnerLoopEnd = endCoordinate.x;

                return swipeData;
            }
    
            private LightSwipeData GetFirstSwipePassData()
            {
                var swipeData = new LightSwipeData();
                swipeData.States = _threadLightData;
        
                //Bottom left cell of the viewport
                var startCoordinate = _threadViewportCoordinates[0];
        
                //Top right cell of the viewport
                var endCoordinate = _threadViewportCoordinates[1];

                swipeData.OuterLoopStart = startCoordinate.x;
                swipeData.OuterLoopEnd = endCoordinate.x;

                swipeData.InnerLoopStart = startCoordinate.y;
                swipeData.InnerLoopEnd = endCoordinate.y;
        
                return swipeData;
            }

            private class LightingInfo
            {
                public int BlockType;
                public int BackgroundType;
                public byte Value;
            }
        }
    }
}