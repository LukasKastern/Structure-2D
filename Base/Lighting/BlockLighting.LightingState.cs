namespace Structure2D.Lighting
{
    public partial class BlockLighting
    {
        /// <summary>
        /// State used as an an abstraction when handling lighting.
        /// </summary>
        internal class LightingState 
        {
            public byte Value;
            public byte BlockID;
            public byte BackgroundID;
        }   
    }
}