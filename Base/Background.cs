using UnityEngine;

namespace Structure2D
{
    /// <summary>
    /// Background used by the BlockData to create texture and meta data from.
    /// </summary>
    [CreateAssetMenu(menuName = "Structure2D/Background", fileName = "Background")]
    public class Background : ScriptableObject
    {
        /// <summary>
        /// This is the unique Identifier for this Block.
        /// This gets set by the BlockData.
        /// If this is -1 the BlockID is not included in the BlockData.
        /// </summary>
        public int ID { get; internal set; } = -1;
        
        /// <summary>
        /// Amount of light this Background blocks.
        /// </summary>
        public byte LightBlockAmount = 20;
    
        /// <summary>
        /// Texture of this Background.
        /// </summary>
        public Texture2D Texture;
        
        /// <summary>
        /// Does the Background block sunlight 
        /// </summary>
        public bool BlocksSunLight = true;

    }
}