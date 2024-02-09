using UnityEngine;

namespace Structure2D.Base.Utility
{
    /// <summary>
    /// Used to log additional debug information.
    /// </summary>
    public static class DebugUtility
    {
        public delegate void DebugTarget (string stringToLog);

        /// <summary>
        /// If this is set to true some classes will print additional information to the debug target.
        /// </summary>
        public static bool DoDebug { get; private set; } = true;
        
        private static DebugTarget _currentTarget;
        
        /// <summary>
        /// Sets the target to which the Debug Utility should print.
        /// </summary>
        /// <param name="target"></param>
        public static void SetDebugTarget(DebugTarget target)
        {
            _currentTarget = target;
        }
        
        /// <summary>
        /// Logs the given text to the current target if DoDebug is true.
        /// </summary>
        internal static void LogString(string textToLog)
        {
            if (!DoDebug)
                return;
                
            _currentTarget?.Invoke(textToLog);
        }
    }
}