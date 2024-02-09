using System;
using System.Collections;
using System.Collections.Generic;
using Structure2D;
using Structure2D.Lighting;
using UnityEngine;

namespace Structure2D
{
    public class BlockCursor : MonoBehaviour
    {
        public static BlockCursor Instance { get; private set; }
        
        [SerializeField]
        private float _zOffset;

        private SpriteRenderer _renderer;
        
        private void Awake()
        {
            InitializeCursor();
            
            SetSize(1);

            if(Instance == null)
                Instance = this;
            
            
        }

        private void InitializeCursor()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }
        

        /// <summary>
        /// Sets the size of the cursor to the amount of blocks
        /// </summary>
        /// <param name="blocks"></param>
        public void SetSize(int blocks)
        {
            _renderer.size = (1 + (((float) blocks - 1) * 2)) * CellMetrics.CellSize * Vector3.one;
        }

        private void Update()
        {
            var position = Coordinate.FromScreenPoint(Input.mousePosition);
            transform.position = Coordinate.ToWorldPoint(position) + Vector3.forward * _zOffset;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}