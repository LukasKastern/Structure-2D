using System;
using Structure2D.MapGeneration;
using UnityEngine;

namespace Structure2D
{
    [RequireComponent(typeof(Camera)), AddComponentMenu("Structure 2D/Example/Camera Controller")]
    public class CameraController : MonoBehaviour
    {
        public bool IsFollowing { get; set; }

        public GameObject FollowTarget { get; set; }
        
        [SerializeField] 
        private Vector3 _followOffset;

        [SerializeField] 
        private float _movementSpeed;

        [SerializeField]
        private float[] _zoomSteps;

        [SerializeField]
        private int _currentZoomStep;

        [SerializeField]
        private KeyCode _zoomInKey;

        [SerializeField]
        private KeyCode _zoomOutKey;

        [SerializeField]
        private float _zoomSmothFactor;
        
        private Camera _camera;

        private string _zoomHelpText
        {
            get => $"Press <color=blue>{_zoomOutKey}</color> and <color=blue>{_zoomInKey}</color> to Zoom";
        }
        
        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            HelpText.AddEntry(_zoomHelpText);
            MapGenerator.FinishedMapGeneration += MapGenerated;
        }

        private void OnDisable()
        {
            HelpText.RemoveEntry(_zoomHelpText);
            MapGenerator.FinishedMapGeneration -= MapGenerated;
        }

        void Update()
        {
            ProcessZoom();
            
            if (IsFollowing)
                return;
            
            transform.Translate(new Vector3(GetHorizontalInput(), GetVerticalInput()));
        }

        private void ProcessZoom()
        {
            if (Input.GetKeyDown(_zoomInKey))
                _currentZoomStep++;
            else if (Input.GetKeyDown(_zoomOutKey))
                _currentZoomStep--;

            _currentZoomStep = Mathf.Clamp(_currentZoomStep, 0, _zoomSteps.Length - 1);


            _camera.orthographicSize = Mathf.MoveTowards(_camera.orthographicSize, _zoomSteps[_currentZoomStep], _zoomSmothFactor * Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (!IsFollowing || FollowTarget == null) 
                return;

            transform.position = FollowTarget.transform.position + _followOffset;
        }

        private void MapGenerated()
        {
            transform.position = new Vector3(CellMap.MapWidth / 2 * CellMetrics.CellSize,  CellMap.MapHeight / 1.3f * CellMetrics.CellSize, transform.position.z);
        }

        private float GetVerticalInput()
        {
            return Input.GetAxisRaw("Vertical") * Time.deltaTime * _movementSpeed;
        }
    
        private float GetHorizontalInput()
        {
            return Input.GetAxis("Horizontal") * Time.deltaTime * _movementSpeed;
        }
    }
    
    
}