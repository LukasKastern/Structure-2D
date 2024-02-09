using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Structure2D.Utility;
using UnityEngine;
using UnityEngine.UI;

public class LoadPanelUI : MonoBehaviour
{
    [SerializeField] 
    private Transform _contentToSpawnTheEntriesTo;

    [SerializeField] 
    private SaveEntry _saveEntryPrefab;

    [SerializeField] 
    private Button _deleteButton;

    [SerializeField] 
    private Button _loadButton;
    
    private Scrollbar _scrollbar;
    
    private string Path => SaveManagerUi.SaveFilePath;

    private List<SaveEntry> _entries = new List<SaveEntry>();

    private SaveManagerUi _managerUi;

    private void Awake()
    {
        _deleteButton.onClick.AddListener(DeleteEntry);
        _loadButton.onClick.AddListener(LoadMap);
    }

    private void RefreshSaveEntries()
    {
        Directory.CreateDirectory(Path);
        
        var safeFiles = Directory.GetFiles(Path, "*.Save");

        foreach (var entry in _entries)
        {
            GameObject.Destroy(entry.gameObject);
        }
        
        _entries.Clear();
        
        foreach (var safeFile in safeFiles)
        {
            using (Stream stream = File.OpenRead(safeFile))
            {
                if(!SaveManager.CanLoadMap(stream, out MapData mapdata))
                    continue;

                var newEntry = GameObject.Instantiate(_saveEntryPrefab, _contentToSpawnTheEntriesTo);
                
                newEntry.LoadFromData(new SaveEntryData() {MapData = mapdata, SavePath = safeFile});
                
                _entries.Add(newEntry);
            }
        }
    }

    private void LoadMap()
    {
        if(!SaveEntry.CurrentlySelectedEntry)
            return;

        using (var stream = File.OpenRead(SaveEntry.CurrentlySelectedEntry.Path))
        {
            SaveManager.LoadMapFromStream(stream);
        }
    }
    
    private void DeleteEntry()
    {
        if(!SaveEntry.CurrentlySelectedEntry)
            return;
        
        File.Delete(SaveEntry.CurrentlySelectedEntry.Path);

        var entry = _entries.Find((i) => i == SaveEntry.CurrentlySelectedEntry);

        _entries.Remove(entry);
        GameObject.Destroy(entry.gameObject);
    }
    
    private void OnEnable()
    {
        if (_managerUi == null)
            _managerUi = GameObject.FindObjectOfType<SaveManagerUi>();
        
        RefreshSaveEntries();
    }

    internal class SaveEntryData
    {
        public string SavePath;
        public MapData MapData;
    }
}