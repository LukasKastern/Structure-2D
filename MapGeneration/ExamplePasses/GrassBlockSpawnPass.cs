using Structure2D;
using Structure2D.MapGeneration;
using Structure2D.MapGeneration.BasePasses;
using UnityEngine;

[System.Serializable]
public class GrassBlockSpawnPass : MapGenerationPass
{
    [SerializeField] 
    private Block _grassBlock;

    [SerializeField] 
    private Background _grassBackground;

    [Range(1, 10)]
    [SerializeField] 
    private int _blockHeight;

    
    public override void Apply(MapGenerator mapGenerator)
    {
        ApplyGrassCells(mapGenerator);
    }
    
    private void ApplyGrassCells(MapGenerator mapGenerator)
    {
        for (int x = 0; x < mapGenerator.MapWidth; ++x)
        {
            var surfaceCellHeight = mapGenerator.GetSurfaceCellHeight(x);
            
            //Not we apply our grass Blocks/Backgrounds for the given Block Height
            for (int y = 0; y < _blockHeight && surfaceCellHeight.Value - y >= 0; ++y)
            {
                var generationData = CellMap.GetCell(x, surfaceCellHeight.Value - y);

                //Before assigning the id's we check if the Block/Background is null or not included in the CellData
                if (_grassBlock != null && _grassBlock.ID != -1)
                    generationData.Block = _grassBlock.ID;
                    
                    
                if (_grassBackground != null && _grassBackground.ID != -1)
                    generationData.Background = _grassBackground.ID;
            }
        }
    }

}