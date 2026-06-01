#if UNITY_EDITOR
namespace HierarchyDesigner
{
    internal static class HD_Constants
    {
        #region Properties
        #region Asset Info
        public const string AssetName = "Hierarchy Designer";
        public const string AssetVersion = "VERSION 3.2.0";
        public const string AssetLocation = "Tools/Hierarchy Designer";
        #endregion

        #region Menu Info
        #region Priority
        public const int MenuPriorityOne = 0;
        public const int MenuPriorityTwo = 11;
        public const int MenuPriorityThree = 22;
        public const int MenuPriorityFour = 33;
        public const int MenuPriorityFive = 44;
        public const int MenuPrioritySix = 55;
        public const int MenuPrioritySeven = 66;
        public const int MenuPriorityEight = 77;
        public const int MenuPriorityNine = 88;
        public const int MenuPriorityTen = 99;
        public const int MenuPriorityEleven = 110;
        public const int MenuPriorityTwelve = 121;
        public const int MenuPriorityThirteen = 132;
        public const int MenuPriorityFourteen = 143;
        public const int MenuPriorityFifteen = 154;
        public const int MenuPrioritySixteen = 165;
        public const int MenuPrioritySeventeen = 176;
        public const int MenuPriorityEigtheen = 187;
        public const int MenuPriorityNineteen = 198;
        public const int MenuPriorityTwenty = 209;
        public const int MenuPriorityTwentyOne = 220;
        #endregion

        #region Item Name
        public const string MenuBase = "GameObject/" + AssetName;
        public const string GroupFolders = MenuBase + "/Folders";
        public const string GroupSeparators = MenuBase + "/Separators";
        public const string GroupTools = MenuBase + "/Tools";
        public const string GroupRefresh = MenuBase + "/Refresh";

        #region Tools' Items
        public const string Tools_Activate = GroupTools + "/Activate";
        public const string Tools_Count = GroupTools + "/Count";
        public const string Tools_Deactivate = GroupTools + "/Deactivate";
        public const string Tools_Lock = GroupTools + "/Lock";
        public const string Tools_Rename = GroupTools + "/Rename";
        public const string Tools_Select = GroupTools + "/Select";
        public const string Tools_Sort = GroupTools + "/Sort";
        public const string Tools_Unlock = GroupTools + "/Unlock";

        #region Activate
        public const string Activate_General = Tools_Activate + "/General";
        public const string Activate_2D = Tools_Activate + "/2D Objects";
        public const string Activate_2D_Sprites = Activate_2D + "/Sprites";
        public const string Activate_2D_Physics = Activate_2D + "/Physics";
        public const string Activate_3D = Tools_Activate + "/3D Objects";
        public const string Activate_3D_Legacy = Activate_3D + "/Legacy";
        public const string Activate_Audio = Tools_Activate + "/Audio";
        public const string Activate_Effects = Tools_Activate + "/Effects";
        public const string Activate_Light = Tools_Activate + "/Light";
        public const string Activate_Video = Tools_Activate + "/Video";
        public const string Activate_UIToolkit = Tools_Activate + "/UI Toolkit";
        public const string Activate_UI = Tools_Activate + "/UI";
        public const string Activate_UI_Legacy = Activate_UI + "/Legacy";
        public const string Activate_UI_Effects = Activate_UI + "/Effects";
        #endregion

        #region Count
        public const string Count_General = Tools_Count + "/General";
        public const string Count_2D = Tools_Count + "/2D Objects";
        public const string Count_2D_Sprites = Count_2D + "/Sprites";
        public const string Count_2D_Physics = Count_2D + "/Physics";
        public const string Count_3D = Tools_Count + "/3D Objects";
        public const string Count_3D_Legacy = Count_3D + "/Legacy";
        public const string Count_Audio = Tools_Count + "/Audio";
        public const string Count_Effects = Tools_Count + "/Effects";
        public const string Count_Light = Tools_Count + "/Light";
        public const string Count_Video = Tools_Count + "/Video";
        public const string Count_UIToolkit = Tools_Count + "/UI Toolkit";
        public const string Count_UI = Tools_Count + "/UI";
        public const string Count_UI_Legacy = Count_UI + "/Legacy";
        public const string Count_UI_Effects = Count_UI + "/Effects";
        #endregion

        #region Deactivate
        public const string Deactivate_General = Tools_Deactivate + "/General";
        public const string Deactivate_2D = Tools_Deactivate + "/2D Objects";
        public const string Deactivate_2D_Sprites = Deactivate_2D + "/Sprites";
        public const string Deactivate_2D_Physics = Deactivate_2D + "/Physics";
        public const string Deactivate_3D = Tools_Deactivate + "/3D Objects";
        public const string Deactivate_3D_Legacy = Deactivate_3D + "/Legacy";
        public const string Deactivate_Audio = Tools_Deactivate + "/Audio";
        public const string Deactivate_Effects = Tools_Deactivate + "/Effects";
        public const string Deactivate_Light = Tools_Deactivate + "/Light";
        public const string Deactivate_Video = Tools_Deactivate + "/Video";
        public const string Deactivate_UIToolkit = Tools_Deactivate + "/UI Toolkit";
        public const string Deactivate_UI = Tools_Deactivate + "/UI";
        public const string Deactivate_UI_Legacy = Deactivate_UI + "/Legacy";
        public const string Deactivate_UI_Effects = Deactivate_UI + "/Effects";
        #endregion

        #region Lock
        public const string Lock_General = Tools_Lock + "/General";
        public const string Lock_2D = Tools_Lock + "/2D Objects";
        public const string Lock_2D_Sprites = Lock_2D + "/Sprites";
        public const string Lock_2D_Physics = Lock_2D + "/Physics";
        public const string Lock_3D = Tools_Lock + "/3D Objects";
        public const string Lock_3D_Legacy = Lock_3D + "/Legacy";
        public const string Lock_Audio = Tools_Lock + "/Audio";
        public const string Lock_Effects = Tools_Lock + "/Effects";
        public const string Lock_Light = Tools_Lock + "/Light";
        public const string Lock_Video = Tools_Lock + "/Video";
        public const string Lock_UIToolkit = Tools_Lock + "/UI Toolkit";
        public const string Lock_UI = Tools_Lock + "/UI";
        public const string Lock_UI_Legacy = Lock_UI + "/Legacy";
        public const string Lock_UI_Effects = Lock_UI + "/Effects";
        #endregion

        #region Select
        public const string Select_General = Tools_Select + "/General";
        public const string Select_2D = Tools_Select + "/2D Objects";
        public const string Select_2D_Sprites = Select_2D + "/Sprites";
        public const string Select_2D_Physics = Select_2D + "/Physics";
        public const string Select_3D = Tools_Select + "/3D Objects";
        public const string Select_3D_Legacy = Select_3D + "/Legacy";
        public const string Select_Audio = Tools_Select + "/Audio";
        public const string Select_Effects = Tools_Select + "/Effects";
        public const string Select_Light = Tools_Select + "/Light";
        public const string Select_Video = Tools_Select + "/Video";
        public const string Select_UIToolkit = Tools_Select + "/UI Toolkit";
        public const string Select_UI = Tools_Select + "/UI";
        public const string Select_UI_Legacy = Select_UI + "/Legacy";
        public const string Select_UI_Effects = Select_UI + "/Effects";
        #endregion

        #region Unlock
        public const string Unlock_General = Tools_Unlock + "/General";
        public const string Unlock_2D = Tools_Unlock + "/2D Objects";
        public const string Unlock_2D_Sprites = Unlock_2D + "/Sprites";
        public const string Unlock_2D_Physics = Unlock_2D + "/Physics";
        public const string Unlock_3D = Tools_Unlock + "/3D Objects";
        public const string Unlock_3D_Legacy = Unlock_3D + "/Legacy";
        public const string Unlock_Audio = Tools_Unlock + "/Audio";
        public const string Unlock_Effects = Tools_Unlock + "/Effects";
        public const string Unlock_Light = Tools_Unlock + "/Light";
        public const string Unlock_Video = Tools_Unlock + "/Video";
        public const string Unlock_UIToolkit = Tools_Unlock + "/UI Toolkit";
        public const string Unlock_UI = Tools_Unlock + "/UI";
        public const string Unlock_UI_Legacy = Unlock_UI + "/Legacy";
        public const string Unlock_UI_Effects = Unlock_UI + "/Effects";
        #endregion
        #endregion
        #endregion
        #endregion

        #region General Info 
        public const string HierarchyWindow = "Hierarchy";
        public const string InspectorWindow = "InspectorWindow";
        #endregion

        #region Separator Info
        public const string SeparatorTag = "EditorOnly";
        public const string SeparatorPrefix = "//";
        #endregion

        #region Folder Names
        public const string EditorFolderName = "Editor";
        public const string DocumentationFolderName = "Documentation";
        public const string SavedDataFolderName = "Saved Data";
        public const string ScriptsFolderName = "Scripts";
        #endregion

        #region File Names
        public const string PatchNotesTextFileName = "Hierarchy Designer Patch Notes.txt";
        public const string ConstantClassTextFileName = "HD_Common_Constants";
        public const string FolderSettingsTextFileName = "HierarchyDesigner_SavedData_Folders.json";
        public const string SeparatorSettingsTextFileName = "HierarchyDesigner_SavedData_Separators.json";
        public const string CustomPresetsTextFileName = "HierarchyDesigner_SavedData_CustomPresets.json";
        public const string GeneralSettingsTextFileName = "HierarchyDesigner_SavedData_GeneralSettings.json";
        public const string DesignSettingsTextFileName = "HierarchyDesigner_SavedData_DesignSettings.json";
        public const string ShortcutSettingsTextFileName = "HierarchyDesigner_SavedData_ShortcutsSettings.json";
        public const string AdvancedSettingsTextFileName = "HierarchyDesigner_SavedData_AdvancedSettings.json";
        public const string FavoriteScenesTextFileName = "HierarchyDesigner_SavedData_FavoriteScenes.json";
        #endregion
        #endregion
    }
}
#endif