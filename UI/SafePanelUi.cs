using System;
using System.IO;
using Structure2D.Utility;
using UnityEngine;
using UnityEngine.UI;

public class SafePanelUi : MonoBehaviour
{
    [SerializeField] 
    private InputField _mapName;

    [SerializeField] 
    private Button _safeButton;

    internal Action OnSavedMap;

    private void Awake()
    {
        _safeButton.onClick.AddListener(SafeMap);
    }

    private void SafeMap()
    {
        var path = SaveManagerUi.SaveFilePath + _mapName.text + "." + SaveManagerUi.SafeFileExtensions;
        
        using (var stream = File.Create(path))
        {
            SaveManager.SaveMapToStream(stream);
        }
        
        OnSavedMap?.Invoke();
    }
}