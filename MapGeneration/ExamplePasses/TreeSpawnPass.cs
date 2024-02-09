using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Structure2D.MapGeneration.BasePasses
{
    [System.Serializable]
    public class TreeSpawnPass : SurfaceObjectSpawnPass
    {
        public const int TreeSpawnPassPriority = 15;
        
        /// <summary>
        /// Max amount of parts a tree can have, without root and top.
        /// </summary>
        public int MaxTreeParts;
        
        /// <summary>
        /// Min amount of parts a tree can have, without root and top.
        /// </summary>
        public int MinTreeParts;
        
        [SerializeField] 
        private SpriteRenderer _treeRoot;
        
        [SerializeField]
        private SpriteRenderer _treeTop;
        
        [SerializeField]
        private SpriteRenderer[] _treeParts;
        
        private ObjectSpawnData[] _parts;
        
        private Vector3 _currentSpawnPosition;

        public override void PrepareGeneration()
        {
            _parts = new ObjectSpawnData[2 + _treeParts.Length];

            _parts[0] = GenerateTreeData(_treeRoot);
            _parts[1] = GenerateTreeData(_treeTop);

            for (int i = 0; i < _treeParts.Length; ++i)
            {
                _parts[2 + i] = GenerateTreeData(_treeParts[i]);
            }
            
        }

        private ObjectSpawnData GenerateTreeData(SpriteRenderer renderer)
        {
            return new ObjectSpawnData
            {
                ObjectToSpawn = renderer.gameObject,
                SpawnHeight = (renderer.size.y / 2) * renderer.transform.localScale.y,
            };
        }

        protected override void SpawnObject(Coordinate spawnCoordinate, MapGenerator mapGenerator)
        {
            _currentSpawnPosition = Coordinate.ToWorldPoint(spawnCoordinate, CoordinateAnchor.UpperCenter);
            
            var tree = mapGenerator.SpawnGameObject(null, SurfaceObjectSpawnPass.GetLayerParent(SpawnLayer, mapGenerator));
            tree.Position = _currentSpawnPosition;
            tree.Name = "Tree";
            tree.Types = new Type[] {typeof(CellObjectLoader)};
            

            SpawnTreePart(_parts[0], tree, mapGenerator);
                
            var treeHeight = mapGenerator.MapGenRandom.Next(MinTreeParts, MaxTreeParts);

            for (int i = 0; i < treeHeight; ++i)
            {
                SpawnTreePart(GetRandomTreePart(mapGenerator), tree, mapGenerator);   
            }
                
            SpawnTreePart(_parts[1], tree, mapGenerator);
        }

        private MapGenerator.ObjectSpawnQueueData SpawnTreePart(ObjectSpawnData part, MapGenerator.ObjectSpawnQueueData parent, MapGenerator generator)
        {
            _currentSpawnPosition.y += part.SpawnHeight;
            
            var queueData = generator.SpawnGameObject(part.ObjectToSpawn, _currentSpawnPosition , Quaternion.identity, parent);
            
            _currentSpawnPosition.y += part.SpawnHeight;
            
            return queueData;
        }

        private ObjectSpawnData GetRandomTreePart(MapGenerator mapGenerator)
        {
            int index = mapGenerator.MapGenRandom.Next(2, _parts.Length);

            return _parts[index];
        }
    }
}