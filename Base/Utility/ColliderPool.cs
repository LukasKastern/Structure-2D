using System.Collections.Generic;
using UnityEngine;

namespace Structure2D.Utility
{
    internal static class ColliderPool
    {
        private static Stack<EdgeCollider2D> stack = new Stack<EdgeCollider2D>();

        private static GameObject _container;
    
        internal static EdgeCollider2D Get () {
      
            if (!_container)
            {
                _container = new GameObject("ColliderContainer") {hideFlags = HideFlags.None};
            }

            if (stack.Count == 0) return _container.AddComponent<EdgeCollider2D>();
        
            var collider = stack.Pop();
            collider.enabled = true;
            return collider;
        }
    
        internal static void Add (EdgeCollider2D collider)
        {
            collider.enabled = false;
            stack.Push(collider);
        }
    }
}