using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structure2D
{
    [AddComponentMenu("Structure 2D/Parallax/Scroll Manager")]
    public class ParallaxScrollManager : MonoBehaviour
    {
        public ParallaxScroller[] ObjectsToScroll;
    
        [SerializeField] 
        private GameObject _viewer;

        public float PlayBaseYPosition;

        public float YScrolLFactor;
    
        /// <summary>
        /// Sets the viewer of which we base the scrolling on
        /// </summary>
        /// <param name="newViewer"></param>
        public void SetViewer(GameObject newViewer)
        {
            _viewer = newViewer;
        }

        private void Update()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, (_viewer.transform.position.y - PlayBaseYPosition) * YScrolLFactor, transform.localPosition.z);
        
            foreach (var objectToScroll in ObjectsToScroll)
            {
                objectToScroll.Scroll(_viewer);
            }
        }
    }
}