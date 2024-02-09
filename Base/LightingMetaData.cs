using Structure2D.Lighting;
using UnityEngine;

namespace Structure2D
{
    /// <summary>
    /// Lighting Meta Data stores information about the lighting data of Blocks and Backgrounds.
    /// </summary>
    public class LightingMetaData : MetaDataBaseClass
    {
        public static byte[] BackgroundsLightBlockAmount;
        public static byte[] BlocksLightBlockAmount;
        public static bool[] DoesBackgroundTypeBlockSunlight;

        private static byte MinBlockAmount => 255 / BlockLighting.MaxLightDistance;
        
        public override void RegisterForBlockInitialization(int size)
        {
            BlocksLightBlockAmount = new byte[size];
        }
 
        public override void EnumerateBlockInitialization(Block block)
        {
            if (block.LightBlockAmount < MinBlockAmount)
                block.LightBlockAmount = (byte)MinBlockAmount;
                
            BlocksLightBlockAmount[block.ID] = block.LightBlockAmount;
        }
 
        public override void RegisterForBackgroundInitialization(int size)
        {
            BackgroundsLightBlockAmount = new byte[size];
            DoesBackgroundTypeBlockSunlight = new bool[size];
        }
 
        public override void EnumerateBackgroundInitialization(Background background)
        {
            if (background.LightBlockAmount < MinBlockAmount)
                background.LightBlockAmount = MinBlockAmount;
            
            BackgroundsLightBlockAmount[background.ID] = background.LightBlockAmount;
            DoesBackgroundTypeBlockSunlight[background.ID] = background.BlocksSunLight;
        }
    }
}