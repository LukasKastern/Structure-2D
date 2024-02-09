using System;
using System.Collections.Generic;
using UnityEngine;

namespace Structure2D
{
    [AddComponentMenu("Structure 2D/Object Loader")]
    public class CellObjectLoader : MonoBehaviour
    {
        private static bool _isSubscribedToChunkLoader;

        private static Dictionary<Vector2Int, List<GameObject>> _objectsListeningForLoading =
            new Dictionary<Vector2Int, List<GameObject>>();

        private bool _isRegistered;

        private static HashSet<Vector2Int> _loadedChunks = new HashSet<Vector2Int>();
        
        public void SetMappedPosition(Vector2Int newPosition)
        {
            if (_isRegistered)
            {
                UnRegister();
            }

            Register(newPosition);
        }

        private Vector2Int _currentPosition;

        private void Awake()
        {
            if (!_isSubscribedToChunkLoader)
            {
                _isSubscribedToChunkLoader = true;

                ChunkLoader.ChunkLoaded += ChunkLoaded;
                ChunkLoader.ChunkUnloaded += ChunkUnloaded;
                CellMap.MapUnloaded += Clear;
            }
        }

        private void Start()
        {
            SetMappedPosition(CurrentChunkPosition());
        }

        private static void ChunkLoaded(Vector2Int chunk)
        {
            _loadedChunks.Add(chunk);

            if(!_objectsListeningForLoading.TryGetValue(chunk, out var objects))
                return;

            for (var index = 0; index < objects.Count; index++)
            {
                var objectToChangeStateOf = objects[index];
                objectToChangeStateOf.SetActive(true);
            }
        }

        /// <summary>
        /// This destroy all Cell Objects and clears the dictionary
        /// </summary>
        private static void Clear()
        {
            foreach (var objects in _objectsListeningForLoading)
            {
                foreach (var objectToDestroy in objects.Value)
                {
                    GameObject.Destroy(objectToDestroy);
                }
                
                objects.Value.Clear();
            }
        }
        
        private static void ChunkUnloaded(Vector2Int chunk)
        {
            _loadedChunks.Remove(chunk);
            
            if(!_objectsListeningForLoading.TryGetValue(chunk, out var objects))
                return;

            for (var index = 0; index < objects.Count; index++)
            {
                var objectToChangeStateOf = objects[index];
                objectToChangeStateOf.SetActive(false);
            }
        }

        private Vector2Int CurrentChunkPosition()
        {
            return CellMap.WorldPointToChunkOffset(transform.position);
        }
        
        private void Register(Vector2Int newPosition)
        {
            _currentPosition = newPosition;

            _isRegistered = true;

            if (!_objectsListeningForLoading.TryGetValue(_currentPosition, out var objects))
            {
                objects = new List<GameObject>();
                _objectsListeningForLoading.Add(_currentPosition, objects);
            }
            
            objects.Add(this.gameObject);
            
            ChangeObjectState(_loadedChunks.Contains(_currentPosition));
        }

        private void ChangeObjectState(bool state)
        {
            gameObject.SetActive(state);
        }

        private void UnRegister()
        {
            if (!_objectsListeningForLoading.TryGetValue(_currentPosition, out var objects) || !objects.Remove(this.gameObject))
            {
                Debug.LogWarning("Tried to unregister object without being registered");
                return;
            }

            _isRegistered = false;
        }
    }
}