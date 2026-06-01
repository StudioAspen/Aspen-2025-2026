#if UNITY_EDITOR
using UnityEditor;

namespace HierarchyDesigner
{
    [InitializeOnLoad]
    internal static class HD_Initializer
    {
        static HD_Initializer()
        {
            HD_Editor.LoadCache();
            HD_Settings.Initialize();
            HD_Editor.Initialize();
            HD_Folders.Initialize();
            HD_Separators.Initialize();
            HD_Manager.Initialize();
            HD_Icon.Initialize();
            HD_Presets.Initialize();
            HD_Header.Initialize();
        }
    }
}
#endif