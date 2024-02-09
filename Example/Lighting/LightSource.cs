using System.Collections;
using System.Collections.Generic;
using Structure2D;
using Structure2D.Lighting;
using UnityEngine;

namespace Structure2D.Example
{
    [AddComponentMenu("Structure 2D/Lighting/Light Source")]
    public class LightSource : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] 
        private float _intensity;

        private Coordinate _currentPosition;

        private void Update()
        {
            if (transform.hasChanged)
            {
                GetCurrentPosition();
                transform.hasChanged = false;
            }
        
            BlockLighting.AddTemporaryLight(_currentPosition, _intensity);
        }

        private void GetCurrentPosition()
        {
            _currentPosition = Coordinate.FromWorldPoint(transform.position);
        }
    }
    
}
