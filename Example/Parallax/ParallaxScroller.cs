using UnityEngine;

namespace Structure2D
{
    [AddComponentMenu("Structure 2D/Parallax/Scroller")]
    public class ParallaxScroller : MonoBehaviour
    {
        [SerializeField] 
        private float _overTimeScrollSpeed;
    
        /// <summary>
        /// This is how much the Scroller should scroll for every unit the Viewer moved on the X
        /// </summary>
        [SerializeField] 
        private float _viewerScrollSpeed;

        [SerializeField] 
        private float _viwerYScrollSpeed;
    

        private Vector2 _startOffset;

        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();

            _startOffset = _renderer.sharedMaterial.mainTextureOffset;
        }

        private float GetViewerXPosition(GameObject Viewer)
        {
            return _viewerScrollSpeed * Viewer.transform.position.x;
        }

        private float GetViewerYPosition(GameObject Viewer)
        {
            return _viwerYScrollSpeed * (Viewer.transform.position.y -
                                         FindObjectOfType<ParallaxScrollManager>().PlayBaseYPosition);
        }

        /// <summary>
        /// Scrolls the texture offset of this renderer
        /// </summary>
        /// <param name="Viewer">Viewer by which we scroll the offset</param>
        public void Scroll(GameObject Viewer)
        {
            var overTimeScroll = Mathf.Repeat(Time.time * _overTimeScrollSpeed, 1);
            var viewerScroll = GetViewerXPosition(Viewer);
            var offset = new Vector2(_startOffset.x + overTimeScroll + viewerScroll, _startOffset.y);

            _renderer.sharedMaterial.mainTextureOffset = offset;
        }
    }   
}