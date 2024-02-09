using System;
using UnityEngine;
using UnityEngine.UI;

namespace Structure2D.MapGeneration
{
    [RequireComponent(typeof(Button))]
    public class MapGeneratorUi : MonoBehaviour
    {
        [SerializeField] 
        private MapGenerator _mapGenerator;

        private void Awake()
        {
            var button = GetComponent<Button>();

            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(LoadMap);
        }

        private void Start()
        {
            if(_mapGenerator != null)
                 return;

            _mapGenerator = GameObject.FindObjectOfType<MapGenerator>();

            if (_mapGenerator == null)
            {
                Debug.LogError("{0} couldn't find a MapGenerator object inside the scene");
            }
        }

        public void LoadMap()
        {
            if(_mapGenerator == null)
                return;
            
            _mapGenerator.GenerateMap();
        }
    }
}