using System;
using UnityEngine;

namespace Structure2D.MapGeneration.BasePasses
{
    public class ExamplePassSubscriber : MonoBehaviour
    {
        [SerializeField] 
        private PlayerSpawnPass _playerSpawnPass;
        
        [SerializeField]
        private SurfaceObjectSpawnPass _grassSpawnPass;

        [SerializeField]
        private GrassBlockSpawnPass _grassBlockSpawnPass;
        
        [SerializeField] 
        private SurfaceObjectSpawnPass _plantSpawnpass;
        
        [SerializeField] 
        private TreeSpawnPass _treeSpawnPass;
        
        [SerializeField]
        private BlockSpawnData _coalSpawnData;

        [SerializeField] 
        private BlockSpawnData _stoneSpawnData;
        
        private void OnEnable()
        {
            MapGenerator.PreparePasses += AddPasses;
        }

        private void OnDestroy()
        {
            MapGenerator.PreparePasses -= AddPasses;
        }

        private void AddPasses(MapGenerator generator)
        {
            generator.AddPass(new DefaultBlockSpawnPass(_coalSpawnData), 5);
            generator.AddPass(new DefaultBlockSpawnPass(_stoneSpawnData), 5);
            generator.AddPass(_grassSpawnPass, 10);
            generator.AddPass(_plantSpawnpass, 10);
            generator.AddPass(_treeSpawnPass, 50);
            generator.AddPass(_grassBlockSpawnPass, 50);
            generator.AddPass(_playerSpawnPass, 100);
        }
        
    }
}