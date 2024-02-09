using System;
using UnityEngine;
using UnityEngine.UI;

public class SaveManagerUi : MonoBehaviour
{
    internal static string SaveFilePath => Application.dataPath + "/Saves/";

    internal  const string SafeFileExtensions = "Save";

    [SerializeField] 
    private Button _saveMapEntry;

    [SerializeField] 
    private SafePanelUi _savePanel;

    [SerializeField] 
    private LoadPanelUI _loadPanel;
    
    private void Awake()
    {
        _saveMapEntry.onClick.AddListener(OpenSafePanel);
        _savePanel.OnSavedMap += SavedMap;
        HelpText.AddEntry("Press <color=blue>F1</color> to open the Save/Load Menu");

    }

    private void OnEnable()
    {
        HelpText.AddEntry("Press <color=blue>F1</color> to open the Save/Load Menu");
    }

    private void OnDisable()
    {
        HelpText.RemoveEntry("Press <color=blue>F1</color> to open the Save/Load Menu");
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F1)) return;

        _loadPanel.gameObject.SetActive(!_loadPanel.gameObject.activeSelf);
        _savePanel.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        SetSaveMapEntryToLastChild();
    }

    private void SetSaveMapEntryToLastChild()
    {
        _saveMapEntry.transform.SetAsLastSibling();
    }

    private void OpenSafePanel()
    {
        _savePanel.gameObject.SetActive(true);
        _loadPanel.gameObject.SetActive(false);
    }

    private void SavedMap()
    {
        _savePanel.gameObject.SetActive(false);
        _loadPanel.gameObject.SetActive(false);
    }
}