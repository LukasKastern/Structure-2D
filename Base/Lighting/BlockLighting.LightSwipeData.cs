namespace Structure2D.Lighting
{
    public partial class BlockLighting
    {

        private partial class LightingThread
        {
            private struct LightSwipeData
            {
                public LightingInfo[][] States;
                public int OuterLoopStart;
                public int OuterLoopEnd;

                public int InnerLoopStart;
                public int InnerLoopEnd;
            }     
        }
    }
}