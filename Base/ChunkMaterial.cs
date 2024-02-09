using UnityEngine;

namespace Structure2D
{
    [CreateAssetMenu(menuName = "Structure2D/Chunk Material", fileName = "ChunkMaterial")]
    internal class ChunkMaterial : ScriptableObject
    {
        public Material Material => _material;
        
        [SerializeField]
        private Material _material;
    }
}