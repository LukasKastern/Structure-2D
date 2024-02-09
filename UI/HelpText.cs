using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HelpText : MonoBehaviour
{
    [SerializeField]
    private Transform _contentParent;

    [SerializeField] 
    private Text _entryPrefab;
    
    private static List<string> desiredEntries = new List<string>();
    
    private List<Text> _actualEntries = new List<Text>();
    
    List<Text> _entriesToRemove = new List<Text>();
    
    private void OnEnable()
    {
        _contentParent.gameObject.SetActive(false);
    }

    private void CreateEntriesIfNecessary()
    {
        var entryTextValues = _actualEntries.Select(i => i.text);
        
        foreach (var desiredEntry in desiredEntries)
        {
            if(entryTextValues.Contains(desiredEntry))
               continue;

            var entry = GameObject.Instantiate(_entryPrefab, _contentParent);
            entry.text = desiredEntry;
            _actualEntries.Add(entry);
        }

        
        _entriesToRemove = new List<Text>();
        
        foreach (var actualEntry in _actualEntries)
        {
            if (desiredEntries.Contains(actualEntry.text))
                continue;
            
            _entriesToRemove.Add(actualEntry);
        }

        foreach (var entry in _entriesToRemove)
        {
            _actualEntries.Remove(entry);
            GameObject.Destroy(entry.gameObject);
        }
        
        _entriesToRemove.Clear();
    }
    
    private void Update()
    {
        CreateEntriesIfNecessary();
        
        if(!Input.GetKeyDown(KeyCode.F12))
            return;
        
        _contentParent.gameObject.SetActive(!_contentParent.gameObject.activeSelf);
    }

    public static void AddEntry(string textToShow)
    {
        desiredEntries.Add(textToShow);
    }

    public static void RemoveEntry(string textToRemove)
    {
        desiredEntries.Remove(textToRemove);
    }
}
