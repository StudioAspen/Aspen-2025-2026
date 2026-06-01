#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_Validation
    {
        #region Properties
        private static bool? _isTMPAvailable;
        #endregion

        #region Methods
        public static bool IsTMPAvailable()
        {
            if (!_isTMPAvailable.HasValue)
            {
                _isTMPAvailable = AssetDatabase.FindAssets("t:TMP_Settings").Length > 0;
            }
            return _isTMPAvailable.Value;
        }

        public static T ParseEnum<T>(string value, T defaultValue) where T : struct
        {
            if (Enum.TryParse(value, out T result))
            {
                return result;
            }
            else
            {
                Debug.LogWarning($"Warning: Failed to parse enum of type '{typeof(T)}' from value '{value}'. Falling back to default value '{defaultValue}'.");
                return defaultValue;
            }
        }
        #endregion
    }
}
#endif