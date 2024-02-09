using System;
using Structure2D.Lighting;
using UnityEngine;
using UnityEngine.UI;

namespace Structure2D
{
    /// <summary>
    /// This class can be used to display information about the currently hovered Block.
    /// </summary>
    public class BlockInfo : MonoBehaviour
    {
        [SerializeField] 
        private Text _blockTypeValue;

        [SerializeField] 
        private Text _backgroundTypeValue;

        [SerializeField] 
        private Text _coordinate;

        [SerializeField] 
        private Text _light;
        
        private Camera _camera;
    
        private void Start()
        {
            _camera = Camera.main;

            if (_blockTypeValue != null && _backgroundTypeValue != null && _coordinate != null) return;
            
            Debug.LogErrorFormat("{0} is missing a reference to a Text component", this.name);
            this.enabled = false;
        }

        private void LateUpdate()
        {
            var selectedBlock = CellMap.GetCellAtWorldPoint(_camera.ScreenToWorldPoint(Input.mousePosition));
        
            if(selectedBlock == null)
                return;

            
            _blockTypeValue.text = selectedBlock.Block.ToString();
            _backgroundTypeValue.text = selectedBlock.Background.ToString();
            _coordinate.text = selectedBlock.Coordinate.ToString();
            //_light.text = ((float)LightMap.GetLight(selectedBlock.Coordinate) / 255f).ToString("0.00");
        }
    }
   
}
