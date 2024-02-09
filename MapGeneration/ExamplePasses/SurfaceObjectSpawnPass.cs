using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Structure2D.MapGeneration.BasePasses
{
    [System.Serializable]
    public class SurfaceObjectSpawnPass : MapGenerationPass
    {
        [SerializeField] 
        protected int AmountOfObjectsToSpawn;
        
        [SerializeField]
        private SpriteRenderer[] ObjectsToSpawn;

        [SerializeField] 
        protected  int MinSpawnDistance;

        [SerializeField] 
        protected int MaxSpawnDistance;

        [SerializeField] 
        protected int SpawnLayer;

        protected ObjectSpawnData[] _spawnAbleObjects;

        private static Dictionary<int, HashSet<int>> _layers = new Dictionary<int, HashSet<int>>();

        private static Dictionary<int, MapGenerator.ObjectSpawnQueueData> _layerParents = new Dictionary<int, MapGenerator.ObjectSpawnQueueData>();

        private static bool _isResetQueued;        
        
        private static void ResetLayers()
        {
            _isResetQueued = false;
            MapGenerator.FinishedMapGeneration -= ResetLayers;

            _layerParents = new Dictionary<int, MapGenerator.ObjectSpawnQueueData>();
            _layers = new Dictionary<int, HashSet<int>>();
        }
        
        protected static MapGenerator.ObjectSpawnQueueData GetLayerParent(int layer, MapGenerator mapGenerator)
        {
            if (_layerParents.TryGetValue(layer, out var parent)) return parent;
            
            parent = mapGenerator.SpawnGameObject(null);
            parent.Name = "Layer " + layer;   
            _layerParents.Add(layer, parent);

            return parent;
        }
        
        public override void PrepareGeneration()
        {
            if (!_isResetQueued)
            {
                _isResetQueued = true;
                MapGenerator.FinishedMapGeneration += ResetLayers;
            }
            
            _spawnAbleObjects = new ObjectSpawnData[ObjectsToSpawn.Length];
            
            for (int i = 0; i < ObjectsToSpawn.Length; i++)
            {
                _spawnAbleObjects[i] = new ObjectSpawnData()
                {
                    ObjectToSpawn = ObjectsToSpawn[i].gameObject,
                    SpawnHeight = (ObjectsToSpawn[i].size.y / 2) * ObjectsToSpawn[i].transform.localScale.y
                };
            }        
        }

        public override void Apply(MapGenerator mapGenerator)
        {
            SpawnObjects(mapGenerator);
        }

        /// <summary>
        /// This returns an array of random usable spawn points,
        /// which each have a random distance in range of min and max spawn distance to one another.
        /// </summary>
        protected virtual int[] GetRandomSpawnPoints(MapGenerator mapGenerator)
        {
            List<int> _spawnPoints = new List<int>();

            if (!_layers.TryGetValue(SpawnLayer, out var usedSpawnPoints))
            {
                usedSpawnPoints = new HashSet<int>();
                _layers.Add(SpawnLayer, usedSpawnPoints);
            }

            for (int x = 0; x < mapGenerator.MapWidth; ++x)
            {
                if(usedSpawnPoints.Contains(x))
                    continue;
                else
                    x += mapGenerator.MapGenRandom.Next(MinSpawnDistance, MaxSpawnDistance);

                if (x > mapGenerator.MapWidth - 1)
                    break;

                _spawnPoints.Add(x);
                usedSpawnPoints.Add(x);
            }

            return _spawnPoints.OrderBy(i => mapGenerator.MapGenRandom.Next()).ToArray();
        }

        private void SpawnObjects(MapGenerator generator)
        {
            int leftOverObjectsToSpawn = AmountOfObjectsToSpawn;

            foreach (var spawnPointX in GetRandomSpawnPoints(generator))
            {
                var spawnCoordinate = new Coordinate(spawnPointX, generator.GetSurfaceCellHeight(spawnPointX).Value);
                
                SpawnObject(spawnCoordinate, generator);

                --leftOverObjectsToSpawn;
            }
        }

        /// <summary>
        /// This is called for every object that the pass wants to spawn/.
        /// You can override this to add your own spawn logic. 
        /// </summary>
        protected virtual void SpawnObject(Coordinate spawnCoordinate, MapGenerator mapGenerator)
        {
            var objectToSpawn = GetRandomObjectToSpawn(mapGenerator);

            var spawnPosition = Coordinate.ToWorldPoint(spawnCoordinate, CoordinateAnchor.UpperCenter) + objectToSpawn.SpawnHeight * Vector3.up;

            mapGenerator.SpawnGameObject(objectToSpawn.ObjectToSpawn, spawnPosition, Quaternion.identity, GetLayerParent(SpawnLayer, mapGenerator));
        }


        private ObjectSpawnData GetRandomObjectToSpawn(MapGenerator mapGenerator)
        {
            return _spawnAbleObjects[mapGenerator.MapGenRandom.Next(0, ObjectsToSpawn.Length)];
        }
        
        public struct ObjectSpawnData
        {
            public float SpawnHeight;
            public GameObject ObjectToSpawn;
        }
    }
}