using UnityEngine;

namespace Structure2D.MapGeneration.BasePasses
{
    [System.Serializable]
    public class PlayerSpawnPass : MapGenerationPass
    {
        [SerializeField] 
        private SpriteRenderer _playerObject;

        private SurfaceObjectSpawnPass.ObjectSpawnData SpawnData;

        public override void PrepareGeneration()
        {
            if(_playerObject == null)
                return;

            SpawnData = new SurfaceObjectSpawnPass.ObjectSpawnData()
            {
                SpawnHeight = (_playerObject.size.y * _playerObject.transform.localScale.y) / 2,
                ObjectToSpawn = _playerObject.gameObject
            };
        }

        public override void Apply(MapGenerator mapGenerator)
        {
            var mapMid = mapGenerator.MapWidth / 2;

            var surfaceHeight = mapGenerator.GetSurfaceCellHeight(mapMid);

            if (!surfaceHeight.HasValue || SpawnData.SpawnHeight == 0)
            {
                Debug.LogWarning("Failed spawning player");
                return;
            }
            
            var coordinatePosition = Coordinate.ToWorldPoint(new Coordinate(mapMid, surfaceHeight.Value), CoordinateAnchor.UpperCenter);

            var position = new Vector3(coordinatePosition.x, coordinatePosition.y + SpawnData.SpawnHeight, -5);
            
            mapGenerator.SpawnGameObject(SpawnData.ObjectToSpawn, position, Quaternion.identity, null);
        }

        public override int GetWeight()
        {
            return 1;
        }
    }
}