using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Structure2D.Lighting
{
    /// <summary>
    /// Used to store lighting data for all Cells of the current Map.
    /// </summary>
    public static class LightMap
    {
        private static NativeArray<Color32> _states;
        
        internal static void Initialize()
        {
            if(_states.IsCreated)
                _states.Dispose();
            
            _states = new NativeArray<Color32>(CellMap.MapWidth * CellMap.MapHeight, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }
        

        public static void SetLightMap(Color32[] threadLightValues)
        {
            NativeArray<Color32>.Copy(threadLightValues, _states);
            
        }
        
        public static NativeArray<Color32> GetLightMap()
        {
            return _states;
        }

        public static void Clear()
        {
            _states.Dispose();
        }
    }
}