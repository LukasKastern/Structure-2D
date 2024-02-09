using System;
using System.Collections;
using System.Collections.Generic;
using Structure2D.MapGeneration;
using UnityEngine;
using UnityEngine.UI;

public class MapGeneratingScreen : MonoBehaviour
{
    [SerializeField] 
    private Image _background;
    
    [SerializeField] 
    private Image _loadingBar;
    
    [SerializeField] 
    private Text _loadingProgressText;

    [SerializeField] 
    private MonoBehaviour[] _componentsToChangeStatusWithLoadingScreen;
    
    private void Awake()
    {
        MapGenerator.StartedMapGeneration += ActiveLoadingScreen;
        MapGenerator.FinishedMapGeneration += DisableLoadingScreen;
        DisableLoadingScreen();
    }

    private void OnDestroy()
    {
        MapGenerator.StartedMapGeneration -= ActiveLoadingScreen;
        MapGenerator.FinishedMapGeneration -= DisableLoadingScreen;
    }

    private void LateUpdate()
    {
        var progress = MapGenerator.GenerationProgress;

        _loadingBar.fillAmount = (float)progress / 100;
        _loadingProgressText.text = progress + " %";
    }

    private void DisableLoadingScreen()
    {
        this.enabled = false;
        _background.enabled = false;
        _loadingBar.enabled = false;
        _loadingProgressText.enabled = false;

        foreach (var component in _componentsToChangeStatusWithLoadingScreen)
        {
            component.enabled = false;
        }
    }

    private void ActiveLoadingScreen()
    {
        this.enabled = true;
        _background.enabled = true;
        _loadingBar.enabled = true;
        _loadingProgressText.enabled = true;
        
        
        foreach (var component in _componentsToChangeStatusWithLoadingScreen)
        {
            component.enabled = true;
        }
    }
}
