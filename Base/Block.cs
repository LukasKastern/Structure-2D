using UnityEngine;

namespace Structure2D
{
    /// <summary>
    /// Class used by the BlockData to create texture and meta data from.
    /// </summary>
    [CreateAssetMenu(menuName = "Structure2D/Block", fileName = "Block")]
    public class Block : ScriptableObject
    {
        /// <summary>
        /// This is the unique Identifier for this Block.
        /// This gets set by the BlockData.
        /// If this is -1 the BlockID is not included in the BlockData.
        /// </summary>
        public int ID { get; internal set; } = -1;
        
        /// <summary>
        /// Texture of the Block.
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// Amount of Light this Block blocks;
        /// </summary>
        public byte LightBlockAmount = 40;
        
        /// <summary>
        /// Should the Block be Solid.
        /// </summary>
        public bool IsSolid = true; 
    }
}