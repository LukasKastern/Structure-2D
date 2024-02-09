using System;
using Structure2D.Lighting;
using UnityEngine;

namespace Structure2D.Example
{
    [AddComponentMenu("Structure 2D/Lighting/Day and Night manager")]
    public class DayAndNightManager : MonoBehaviour
    {
        public string CurrentPhase;
        
        [SerializeField]
        private DayPhase[] _phases;

        [SerializeField]
        private MeshRenderer _rendererToChangeTheColorOf;

        [SerializeField]
        private int _startPhase;
      
        private float _currentTime;

        private float _secondsInAFullDay;
        
        
      
        private void Awake()
        {
            for (var index = 0; index < _phases.Length; index++)
            {
                if (index == _startPhase)
                    _currentTime = _secondsInAFullDay;
                
                var phase = _phases[index];
                _secondsInAFullDay += phase.Duration;
            }
           
        }

        private void Update()
        {
            _currentTime += Time.deltaTime;

            _currentTime = Mathf.Repeat(_currentTime, _secondsInAFullDay);

            int _currentPhaseIndex = 0;

            int threshold = 0;
            
            for (int i = 0; i < _phases.Length; ++i)
            {
                threshold += _phases[i].Duration;
                if (_currentTime < threshold)
                {
                    _currentPhaseIndex = i;
                    break;
                }
            }
            ;
            
            DayPhase lastPhase, currentPhase = _phases[_currentPhaseIndex];

            if (_currentPhaseIndex == 0)
                lastPhase = _phases[_phases.Length - 1];

            else
                lastPhase = _phases[_currentPhaseIndex - 1];

            CurrentPhase = currentPhase.Name;

            var currentProgress = _currentTime - threshold + currentPhase.Duration;
            
            var phaseProgress = (currentProgress) / currentPhase.Duration;

            Color lastColor = lastPhase.Color, desiredColor = currentPhase.Color;
            float lastLight = lastPhase.SkyLight, desiredLight = currentPhase.SkyLight;
             
            float currentLight = Mathf.Lerp(lastLight, desiredLight, phaseProgress);
            Color currentColor = Color.Lerp(lastColor, desiredColor, phaseProgress);

            if (_rendererToChangeTheColorOf)
                _rendererToChangeTheColorOf.material.color = currentColor;

            BlockLighting.SkyColor = (byte)(255 * currentLight);
        }

        [System.Serializable]
        public struct DayPhase
        {
            /// <summary>
            /// This is just for documentation purposes.
            /// </summary>
            public string Name;

            public int Duration;

            [Range(0, 1)]
            public float SkyLight;
            
            public Color Color;
        }
    }
}