namespace Structure2D
{
    /// <summary>
    /// Meta Data which Handles the solid state of Blocks.
    /// </summary>
    public class BlockSolidStateMetaData : MetaDataBaseClass
    {
        public static bool[] IsBlockSolid;
     
        public override void RegisterForBlockInitialization(int size)
        {
            IsBlockSolid = new bool[size];
        }
 
        public override void EnumerateBlockInitialization(Block block)
        {
            IsBlockSolid[block.ID] = block.IsSolid;
        }
    }
}