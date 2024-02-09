using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Structure2D.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Structure2D
{
    public class MapEditor : MonoBehaviour
    {
        [SerializeField]
        private InputField _cellTypeInputField;

        [SerializeField]
        private InputField _brushInputField;

        [SerializeField]
        private GameObject _content;

        private bool _doChangeBackground => Input.GetKey(KeyCode.LeftAlt);

        private int _brushSize;
        private int _cellValue;

        private void Start()
        {
            HelpText.AddEntry("Press <color=blue>F2</color> to open/hide the Map Editor");
            _content.gameObject.SetActive(false);

            if (BlockCursor.Instance == null) return;

            BlockCursor.Instance.SetSize(1);
            _brushInputField.onValueChanged.AddListener(OnBrushSizeChanged);
            _cellTypeInputField.onValueChanged.AddListener(OnCellValueChanged);
        }

        private void OnCellValueChanged(string value)
        {
            if (!int.TryParse(value, out var newCellValue))
            {
                newCellValue = 0;
            }

            _cellValue = Mathf.Clamp(newCellValue, 0, 10000);

            _cellTypeInputField.text = _cellValue.ToString();
        }

        private void OnDestroy()
        {
            HelpText.RemoveEntry("Press <color=blue>F2</color> to open/hide the Map Editor");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
                _content.gameObject.SetActive(!_content.activeSelf);

            if (!_content.activeSelf)
                return;

            if (!Input.GetMouseButton(0))
                return;

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            UpdateCells();
        }

        private void OnBrushSizeChanged(string value)
        {
            if (!int.TryParse(value, out var newBrushSize))
            {
                newBrushSize = 0;
            }

            _brushSize = Mathf.Clamp(newBrushSize, 0, 6);

            _brushInputField.text = _brushSize.ToString();

            BlockCursor.Instance.SetSize(_brushSize + 1);
        }

        private void UpdateCells()
        {
            var cells = CellMap.GetCellsInBounds(Input.mousePosition, _brushSize);

            foreach (var cell in cells)
            {
                if (cell == null)
                    return;

                if (_doChangeBackground)
                {
                    if (cell.Background == _cellValue)
                        continue;

                    cell.Background = _cellValue;
                }

                else
                {
                    //If the value didn't change we can just continue
                    if (cell.Block == _cellValue)
                        continue;

                    cell.Block = _cellValue;
                }
            }

            ListPool<Cell>.Add(cells);
        }

    }
}