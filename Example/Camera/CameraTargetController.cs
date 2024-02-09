using System;
using System.Collections;
using System.Collections.Generic;
using Structure2D;
using UnityEngine;

namespace Structure2D.Example
{
    
    /// <summary>
    /// This script toggles the follow mode of the CameraController 
    /// </summary>
    [RequireComponent(typeof(CameraController)), AddComponentMenu("Structure 2D/Example/Camera Target Controller")]
    public class CameraTargetController : MonoBehaviour
    {
        public PlayerController Player
        {
            set
            {
                if (_player == null && value != null && !_hasInfoText)
                {
                    HelpText.AddEntry(InfoText);
                    _hasInfoText = true;
                }

                _camera.FollowTarget = value.gameObject;
                _camera.IsFollowing = value.enabled;
                _player = value;
            }
        }

        private string InfoText => $"Press <color=blue>{_keyToToggle}</color> to Toggle Camera Follow Mode";
    
        [SerializeField]
        private KeyCode _keyToToggle;

        [SerializeField]
        private PlayerController _player;

        private CameraController _camera;

        private bool _hasInfoText;
    
        private void Awake()
        {
            _camera = GetComponent<CameraController>();
        
            _camera.IsFollowing = false;
        }
    
        private void OnDisable()
        {
            if(_hasInfoText)
                HelpText.RemoveEntry(InfoText);
        }

        private void Update()
        {
            if(_player == null)
                return;
        
            if(!Input.GetKeyDown(_keyToToggle))
                return;

            _camera.IsFollowing = !_camera.IsFollowing;

            _player.enabled = _camera.IsFollowing;
        }
    }
}
