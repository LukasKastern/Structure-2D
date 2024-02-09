using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace Structure2D.Lighting
{
    /// <summary>
    /// This class handles the lighting of blocks
    /// </summary>
    [AddComponentMenu("Structure 2D/Framework Base/Block Lighting")]
    public partial class BlockLighting : MonoBehaviour
    {
        /// <summary>
        /// This is the max distance in blocks to which a light source can emit light.
        /// </summary>
        public const int MaxLightDistance = 60;
        
        private static BlockLighting _instance;

        private readonly Dictionary<Coordinate, byte> _tempLight = new Dictionary<Coordinate, byte>();
    
        private readonly LightingThread _lightingThread;

        public static byte SkyColor { get; set; } = 255;

        /// <summary>
        /// Creates a BlockLighting instance if none exists, also initializes the LightMap and the lighting Thread.
        /// </summary>
        internal static void Initialize()
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<BlockLighting>();
                
                if(_instance == null)
                    _instance = new GameObject("LightingManager", typeof(BlockLighting)).GetComponent<BlockLighting>();
            }

            _instance.StartLightingThreadAndInitializeLightMap(); 
        }

        private BlockLighting()
        {
            if (_instance != null)
            {
                Destroy(this.gameObject);
                
                throw new Exception("Tried to create a BlockLighting instance but one already exists," +
                                    "make sure you don't create one in your Scene");
            }
            
            _instance = this;
            
            _lightingThread = new LightingThread();
        }

        private void StartLightingThreadAndInitializeLightMap()
        {
            LightMap.Initialize();
            _instance._lightingThread.Start(CellMap.MapWidth, CellMap.MapHeight);
        }
        
        
        private void LateUpdate()
        {
            if(CellMap.IsMapHidden)
                return;
            
            //Otherwise we fetch the lighting values from the thread
            if (_lightingThread.IsLightingDataAvailable)
            {
                _lightingThread.IsLightingDataAvailable = false;

                FetchThreadLightingValues();
                
                _lightingThread.Reset();
            }

            //If the lighting thread is currently not active we prepare the light values and let the thread process them
            else if (!_lightingThread.IsActive)
            {
                _lightingThread.StartThreadCalculation(_tempLight);
            }
            
            ClearTempLight();
        }
        
        private void FetchThreadLightingValues()
        {
            Profiler.BeginSample("Fetching trhread values");
            
            var threadLightValues = _lightingThread.GetValues();

            LightMap.SetLightMap(threadLightValues);

            Profiler.EndSample();
        }

        private void ClearTempLight()
        {
            _tempLight.Clear();
        }

        /// <summary>
        /// Adds a Temporary Light at the given coordinate.
        ///Temporary Lights get cleared every LateUpdate, so it's necessary to add a new Light every Frame
        /// </summary>
        /// <param name="lightPosition">Coordinate at which the light should be</param>
        /// <param name="light">The amount of light that the coordinate should emit in a range of 0 to 1</param>
        public static void AddTemporaryLight(Coordinate lightPosition, float light)
        {
            if(_instance == null || !Viewport.CurrentViewport.ContainsCoordinate(lightPosition))
                return;
            
            var lightInByte = (byte)(255 * Mathf.Clamp01(light));

            if (_instance._tempLight.TryGetValue(lightPosition, out var tempLight))
            {
                if(tempLight  < lightInByte)
                    _instance._tempLight[lightPosition] = lightInByte;
            }
            
            else
                _instance._tempLight.Add(lightPosition, lightInByte);
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            
            _instance = null;
            LightMap.Clear();
            _lightingThread.Dispose();
        }
    }
}