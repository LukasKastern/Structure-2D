using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Structure2D.Base.Utility;
using Structure2D.Utility.MapGeneration;
using Structure2D.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Structure2D.MapGeneration
{
    /// <summary>
    /// Default Map Generator.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        /// <summary>
        /// This is called when the Map Generator starts generating the Map.
        /// </summary>
        public static Action StartedMapGeneration;
        
        /// <summary>
        /// This is called when the Map Generation has finished.
        /// </summary>
        public static Action FinishedMapGeneration;

        /// <summary>
        /// You can use this to add your passes without an IPassGenerationSubscriber.
        /// Just call AddPass on the given MapGenerator.
        /// </summary>
        public static Action<MapGenerator> PreparePasses;
        
        public MapGeneratorSettings ActiveMapGenSettings { get; private set; }

        private PriorityQueue PriorityQueue;

        private int Seed => ActiveMapGenSettings.Seed;

        /// <summary>
        /// Height in Cells which the Base pass uses.
        /// </summary>
        public int BaseChunkCellHeight { get; private set; }

        /// <summary>
        /// Height in cells which the Air pass uses.
        /// </summary>
        public int AirChunkCellHeight { get; private set; }

        /// <summary>
        /// Height in Cells which the Ground pass uses.
        /// </summary>
        public int GroundCellHeight { get; private set; }

        /// <summary>
        /// Height in Cells at which the Ground pass starts.
        /// </summary>
        public int GroundCellStart { get; private set; }

        /// <summary>
        /// Height in Cells which the Terrain pass uses.
        /// </summary>
        public int TerrainCellHeight { get; private set; }

        /// <summary>
        /// If you need randoms inside a MapGenerationPass you can use this variable.
        /// Unity's' Random class doesn't work on any other thread than the main one.
        /// </summary>
        public System.Random MapGenRandom { get; private set; }

        /// <summary>
        /// Amount of Blocks that are the default Block.
        /// This is used by the Default Block Pass to check if there are enough blocks leftover to spawn. 
        /// </summary>
        public int UsableDefaultBlocks { get; set; }

        private ScriptableGenerationPassSubscriber[] generationSubscribers => ActiveMapGenSettings.PassSubscribers;

        public int ActiveSeed { get; private set; }
        
        private Thread _generationThread;

        private Queue<MapGenerationPass> _passes = new Queue<MapGenerationPass>();

        [SerializeField] private MapGeneratorSettings _defaultSettings;
        
        /// <summary>
        /// These are chunks that get added to the base of the chunks that the map generated
        /// </summary>
        private int BaseChunks => ActiveMapGenSettings.BaseChunks;

        /// <summary>
        /// These are empty chunks which have no cell in them and get added on top of the generated map
        /// </summary>
        private int AirChunks => ActiveMapGenSettings.AirChunks;

        /// <summary>
        /// Map Width in Cells.
        /// </summary>
        public int MapWidth { get; private set; }
        
        /// <summary>
        /// Map Height in Cells.
        /// </summary>
        public int MapHeight { get; private set; }

        private Queue<ObjectSpawnQueueData> _objectsToSpawn = new Queue<ObjectSpawnQueueData>();
        
        private bool ArePassesFinished
        {
            get => (Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 1) == 1);

            set
            {
                if (value) Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 0);

                else Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 0, 1);
            }
        }
        
        /// <summary>
        /// Generation Progress in a range of 0..100
        /// </summary>
        public static long GenerationProgress
        {
            get => Interlocked.Read(ref _currentGenerationProgress);

            set
            {
                value = (long)Mathf.Clamp(value, 0, 100);
                
                Interlocked.Exchange(ref _currentGenerationProgress, value);
            }
        }

        private static long _currentGenerationProgress;
        
        private int _threadSafeBoolBackValue;

        private List<TemporaryMapGenPass> _tempPasses = new List<TemporaryMapGenPass>();

        /// <summary>
        /// Generates a map with the default settings.
        /// </summary>
        public void GenerateMap()
        {
            GenerateMap(_defaultSettings);
        }

        /// <summary>
        /// Generates a map with the given settings.
        /// </summary>
        /// <param name="settings">Settings that the Map Generator should use.</param>
        public void GenerateMap(MapGeneratorSettings settings)
        {
            if(_generationThread != null)
            {
               Debug.LogError("Can't start the Map Generation while a generation thread is already running. /n Listen for the OnMapGenerated callback!"); 
            }
         
            GenerationProgress = 0;

            ActiveMapGenSettings = settings;

            UsableDefaultBlocks = 0;
         
            StartedMapGeneration?.Invoke();

            //Now we wait one frame so visuals like a loading screen, etc can update

            StartCoroutine(StartGeneratingMap());
        }

        private IEnumerator StartGeneratingMap()
        {
            yield return null;

            PrepareMapGeneration();

            ArePassesFinished = false;
            
            _generationThread = new Thread(ApplyPasses);
            _generationThread.Start();
        }

        /// <summary>
        /// Searches for a solid cell starting from a random starting cell
        /// </summary>
        public Cell GetRandomSolidCell()
        {
            PriorityQueue.Clear();

            var notUsableBaseChunkRange = BaseChunkCellHeight;
            int randomRowStart = MapGenRandom.Next(notUsableBaseChunkRange, MapHeight - AirChunkCellHeight);
            int randomColumnStart = MapGenRandom.Next(0, MapWidth);

            var firstCell =
                CellMap.GetCell(randomColumnStart,
                    randomRowStart); //.GetCellUnsafe(randomColumnStart, randomRowStart);
            
            var queueData = new PriorityQueue.QueueData();
            queueData.Cell = firstCell;
            queueData.Distance = 0;
            queueData.SearchHeuristic = 0;

            PriorityQueue.Enqueue(queueData);

            var center = firstCell.Coordinate;

            while (PriorityQueue.Count > 0)
            {
                var currentCell = PriorityQueue.Dequeue(this);

                if (currentCell.Block != (int) BaseBlockTypes.EmptyBlock)
                {
                    PriorityQueue.Clear();
                    return currentCell;
                }

                for (Direction direction = Direction.Up; direction < Direction.UpLeft + 1; ++direction)
                {
                    var neighbor = currentCell.GetNeighbor(direction);

                    if (neighbor == null || !PriorityQueue.IsCellQueueAble(neighbor) ||
                        neighbor.Coordinate.y < notUsableBaseChunkRange) continue;

                    queueData.Cell = neighbor;
                    queueData.Distance = neighbor.Coordinate.DistanceTo(center);
                    queueData.SearchHeuristic = 0;
                    
                    PriorityQueue.Enqueue(queueData);
                }
            }

            PriorityQueue.Clear();
            return null;
        }

        /// <summary>
        /// Returns whether the given block height is higher than the min spawn height and lower than the max spawn height
        /// </summary>
        /// <param name="minSpawnHeight">This is the lowest height in percent where the block will be counted as valid</param>
        /// <param name="maxSpawnHeight">This is the maximum height in percent where the block will be counted as valid</param>
        public bool IsBlockInDesiredHeight(int cellHeight, float minSpawnHeight, float maxSpawnHeight)
        {
            if (cellHeight > (BaseChunkCellHeight + GroundCellHeight + TerrainCellHeight) || cellHeight < BaseChunkCellHeight)
            {
                return false;
            }

            var minBlockInUnits = Mathf.CeilToInt(minSpawnHeight * (MapHeight - AirChunkCellHeight)) + BaseChunkCellHeight;
            var maxBlockInUnits = Mathf.CeilToInt(maxSpawnHeight * MapHeight - AirChunkCellHeight) + minBlockInUnits;

            //Cell height without the base chunks
            var cellHeightInGeneratedMapSpace = cellHeight;

            return cellHeightInGeneratedMapSpace > minBlockInUnits && cellHeightInGeneratedMapSpace < maxBlockInUnits;
        }

        /// <summary>
        /// This sets the actual CellMap cells to our MapGenerationCell.
        /// </summary>
        private void ApplyData()
        {
            CellMap.SetMapVisible();
            
            FinishedMapGeneration?.Invoke();
        }

        private void SpawnObjectsInQueue()
        {
            while (_objectsToSpawn.Count > 0)
            {
                var objectSpawnData = _objectsToSpawn.Dequeue();

                objectSpawnData.Spawn();
            }
        }

        private void Update()
        {
            if(_generationThread == null)
                return;
            
            if(!ArePassesFinished)
                return;

            ArePassesFinished = false;
            _generationThread = null;
            
            ApplyData();
            SpawnObjectsInQueue();
        }
        

        private void PrepareMapGeneration()
        {
            ActiveSeed = Seed;
            
            if (Seed == -1)
            {
                UnityEngine.Random.InitState(Guid.NewGuid().GetHashCode());
                ActiveSeed = UnityEngine.Random.Range(0, int.MaxValue);
            }
            
            MapGenRandom = new System.Random(ActiveSeed);

            MapWidth = ActiveMapGenSettings.MapWidth * CellMetrics.ChunkSize;
            MapHeight = ActiveMapGenSettings.MapHeight * CellMetrics.ChunkSize;

            CellMap.CreateMap(ActiveMapGenSettings.MapWidth, ActiveMapGenSettings.MapHeight);

            NoiseMap.SetScale(ActiveMapGenSettings.NoiseMapXScale, ActiveMapGenSettings.NoiseMapYScale);

            CalculateCellHeights();

            if (PriorityQueue == null)
                PriorityQueue = new PriorityQueue();

            var subscribers = new List<IGenerationPassSubscriber>(generationSubscribers.Length + 1)
            {
                new BasePassSubscriber()
            };
            
            PreparePasses?.Invoke(this);

            subscribers.AddRange(generationSubscribers.OrderBy(i => i.FetchPassOrder()));
            
            foreach (var generationPassSubscriber in subscribers)
            {
                var generationPasses = generationPassSubscriber.GetPasses();

                foreach (var pass in generationPasses)
                {
                    if (pass == null)
                    {
                        Debug.LogError("Tried to add a pass which is null to the Map Generator");
                        continue;
                    }
                    
                    _tempPasses.Add(new TemporaryMapGenPass(pass, generationPassSubscriber.FetchPassOrder()));
                }
            }

            foreach (var tempPass in _tempPasses.OrderBy(i => i.Priority))
            {
                tempPass.Pass.PrepareGeneration();
                _passes.Enqueue(tempPass.Pass);
            }
            
            _tempPasses.Clear();
        }
        
        public void AddPass(MapGenerationPass pass, int priority)
        {
            _tempPasses.Add(new TemporaryMapGenPass(pass, priority));
        }
        
        
        public int? GetSurfaceCellHeight(int column)
        {
            for (int y = MapHeight - 1; y > 0; --y)
            {
                var generationData = CellMap.GetCell(column, y);
                
                if (generationData.Block == (int) BaseBlockTypes.EmptyBlock)
                    continue;

                return y;
            }

            return null;
        }
        
        private void ApplyPasses()
        {
            int passMaxWeight = _passes.Sum(i => i.GetWeight());
            
            int currentWeight = 0;
            
            MapGenerationPass _currentPass = null;

            var watch = Stopwatch.StartNew();

            while (_passes.Count > 0)
            {
                watch.Restart();    
                
                _currentPass = _passes.Dequeue();
                _currentPass.Apply(this);

                currentWeight += Mathf.RoundToInt(100 * ((float)_currentPass.GetWeight() / passMaxWeight));
                    
                GenerationProgress = currentWeight;
                
                DebugUtility.LogString(string.Format("{0} took {1} ms to apply", _currentPass.ToString(), watch.ElapsedMilliseconds));
            }

            ArePassesFinished = true;
        }

        /// <summary>
        /// Use this to spawn GameObjects from passes,
        /// this add the given objects to a queue where it gets spawned as soon as the Map Generation finished.
        /// </summary>
        /// <param name="objectToSpawn"></param>
        public ObjectSpawnQueueData SpawnGameObject(GameObject objectToSpawn)
        {
            return SpawnGameObject(objectToSpawn, Vector3.zero, Quaternion.identity, null);
        }

        public ObjectSpawnQueueData SpawnGameObject(GameObject objectToSpawn, ObjectSpawnQueueData parent)
        {
            return SpawnGameObject(objectToSpawn, Vector3.zero, Quaternion.identity, parent);
        }

        public ObjectSpawnQueueData SpawnGameObject(GameObject objectToSpawn, Vector3 position, Quaternion rotation, ObjectSpawnQueueData parent)
        {
            var spawnQueueData = new ObjectSpawnQueueData()
            {
                ObjectToSpawn = objectToSpawn,
                Position = position,
                Rotation = rotation,
                Parent = parent,
            };
            
            _objectsToSpawn.Enqueue(spawnQueueData);

            return spawnQueueData;
        }
        
        private void CalculateCellHeights()
        {
            BaseChunkCellHeight = BaseChunks * CellMetrics.ChunkSize;
            AirChunkCellHeight = AirChunks * CellMetrics.ChunkSize;
            GroundCellHeight = ActiveMapGenSettings.GroundChunks * CellMetrics.ChunkSize;
            TerrainCellHeight = MapHeight - (BaseChunkCellHeight + AirChunkCellHeight + GroundCellHeight);

            GroundCellStart = BaseChunkCellHeight;
            
            if (TerrainCellHeight < 0)
                throw new Exception("Cell heights were out of bounds");
        }
        
        private struct TemporaryMapGenPass
        {
            public int Priority;
            public MapGenerationPass Pass;

            public TemporaryMapGenPass(MapGenerationPass pass, int priority)
            {
                this.Pass = pass;
                this.Priority = priority;
            }
        }

        /// <summary>
        /// Used to queue the spawning of objects while in a multi-threaded environment.
        /// </summary>
        public class ObjectSpawnQueueData
        {
            /// <summary>
            /// Object to Spawn.
            /// </summary>
            public GameObject ObjectToSpawn;

            public string Name;
            
            /// <summary>
            ///  Parent to spawn to.
            /// </summary>
            public ObjectSpawnQueueData Parent;

            /// <summary>
            /// Desired Spawn Position.
            /// </summary>
            public Vector3 Position;
            
            /// <summary>
            /// Desired Spawn Rotation.
            /// </summary>
            public Quaternion Rotation;

            /// <summary>
            /// This will be resolved as soon as the object is spawned.
            /// </summary>
            public Transform SpawnedObject { get; private set; }

            /// <summary>
            /// These components get added when null is passed as the ObjectToSpawn.
            /// </summary>
            public Type[] Types;
            
            /// <summary>
            /// Spawns the Objects.
            /// </summary>
            internal void Spawn()
            {
                Transform parent = null;

                if (Parent != null)
                {
                    parent = Parent.SpawnedObject;
                }

                if (ObjectToSpawn == null)
                {
                    if (Types != null)
                        SpawnedObject = new GameObject(Name, Types).transform;
                    else
                        SpawnedObject = new GameObject(Name).transform;
    
                    SpawnedObject.parent = parent;
                    SpawnedObject.transform.position = Position;
                    SpawnedObject.transform.rotation = Rotation;
                    return;
                }
                
                SpawnedObject = GameObject.Instantiate(ObjectToSpawn, Position, Rotation, parent).transform;
            }
        }
    }
}
