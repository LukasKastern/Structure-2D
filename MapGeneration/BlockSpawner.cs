using System.Linq;
using Structure2D.MapGeneration.BasePasses;
using UnityEngine;

namespace Structure2D.MapGeneration
{
    /// <summary>
    /// A Pass subscriber which can be used to Spawn Blocks.
    /// </summary>
    [CreateAssetMenu(menuName = "Structure2D/MapGeneration/Pass Subsribers/Block Spawner", fileName = "New Block Spawner")]
    public class BlockSpawner : ScriptableGenerationPassSubscriber
    {
        [SerializeField]
        private BlockSpawnData[] BlocksToSpawn;
        
        public override MapGenerationPass[] GetPasses()
        {
            return BlocksToSpawn.Select(i => new DefaultBlockSpawnPass(i)).ToArray();
        }
        
        public override int FetchPassOrder()
        {
            return 5;
        }
    }
}