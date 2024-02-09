using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveEntry : MonoBehaviour
{
    [SerializeField] 
    private Text _mapNameValue;

    [SerializeField] 
    private Text _mapVersionValue;

    private Button _button;
    
    public static SaveEntry CurrentlySelectedEntry { get; private set; }
    public string Path { get; private set; }

    [SerializeField]
    private Color _selectedColor;

    private Color _idleColor;
    
    private void Awake()
    {
        _idleColor = GetComponent<Image>().color;
        
        _button = gameObject.AddComponent<Button>();

        _button.onClick.AddListener(OnSelected);
    }

    private void OnSelected()
    {
        CurrentlySelectedEntry = this;
    }

    private void Update()
    {
        GetComponent<Image>().color = CurrentlySelectedEntry == this ? _selectedColor : _idleColor;
    }

    private void OnDisable()
    {
        if (CurrentlySelectedEntry == this)
            CurrentlySelectedEntry = null;
    }

    internal void LoadFromData(LoadPanelUI.SaveEntryData data)
    {
        Path = data.SavePath;
        _mapNameValue.text = System.IO.Path.GetFileName(data.SavePath);
        _mapVersionValue.text = data.MapData.MapVersion.ToString();
    }
}