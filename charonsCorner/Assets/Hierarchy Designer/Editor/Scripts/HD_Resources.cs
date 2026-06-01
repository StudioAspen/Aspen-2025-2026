#if UNITY_EDITOR
using System;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_Resources
    {
        private static class ResourceNames
        {
            #region Fonts
            internal const string FontBold = "Hierarchy Designer Font Bold";
            internal const string FontRegular = "Hierarchy Designer Font Regular";
            #endregion

            #region Icons
            internal const string IconResetDark = "Hierarchy Designer Icon Reset Dark";
            internal const string IconResetLight = "Hierarchy Designer Icon Reset Light";
            internal const string IconTooltipDark = "Hierarchy Designer Icon Tooltip Dark";
            internal const string IconTooltipLight = "Hierarchy Designer Icon Tooltip Light";
            internal const string IconLockDark = "Hierarchy Designer Icon Lock Dark";
            internal const string IconLockLight = "Hierarchy Designer Icon Lock Light";
            internal const string IconUnlockDark = "Hierarchy Designer Icon Unlock Dark";
            internal const string IconUnlockLight = "Hierarchy Designer Icon Unlock Light";
            internal const string IconVisibilityOnDark = "Hierarchy Designer Icon Visibility On Dark";
            internal const string IconVisibilityOnLight = "Hierarchy Designer Icon Visibility On Light";
            internal const string IconVisibilityOffDark = "Hierarchy Designer Icon Visibility Off Dark";
            internal const string IconVisibilityOffLight = "Hierarchy Designer Icon Visibility Off Light";
            #endregion

            #region Textures
            internal const string TextureDefault = "Hierarchy Designer Default Texture";
            internal const string TreeBranchIDefault = "Hierarchy Designer Tree Branch Icon Default I";
            internal const string TreeBranchLDefault = "Hierarchy Designer Tree Branch Icon Default L";
            internal const string TreeBranchTDefault = "Hierarchy Designer Tree Branch Icon Default T";
            internal const string TreeBranchTerminalBudDefault = "Hierarchy Designer Tree Branch Icon Default Terminal Bud";
            internal const string TreeBranchICurved = "Hierarchy Designer Tree Branch Icon Curved I";
            internal const string TreeBranchLCurved = "Hierarchy Designer Tree Branch Icon Curved L";
            internal const string TreeBranchTCurved = "Hierarchy Designer Tree Branch Icon Curved T";
            internal const string TreeBranchTerminalBudCurved = "Hierarchy Designer Tree Branch Icon Curved Terminal Bud";
            internal const string TreeBranchIDotted = "Hierarchy Designer Tree Branch Icon Dotted I";
            internal const string TreeBranchLDotted = "Hierarchy Designer Tree Branch Icon Dotted L";
            internal const string TreeBranchTDotted = "Hierarchy Designer Tree Branch Icon Dotted T";
            internal const string TreeBranchTerminalBudDotted = "Hierarchy Designer Tree Branch Icon Dotted Terminal Bud";
            internal const string TreeBranchISegmented = "Hierarchy Designer Tree Branch Icon Segmented I";
            internal const string TreeBranchLSegmented = "Hierarchy Designer Tree Branch Icon Segmented L";
            internal const string TreeBranchTSegmented = "Hierarchy Designer Tree Branch Icon Segmented T";
            internal const string TreeBranchTerminalBudSegmented = "Hierarchy Designer Tree Branch Icon Segmented Terminal Bud";
            internal const string FolderScene = "Hierarchy Designer Folder Icon Scene";
            internal const string FolderDefault = "Hierarchy Designer Folder Icon Default";
            internal const string FolderDefaultOutline = "Hierarchy Designer Folder Icon Default Outline";
            internal const string FolderClassicI = "Hierarchy Designer Folder Icon Classic I";
            internal const string FolderClassicII = "Hierarchy Designer Folder Icon Classic II";
            internal const string FolderClassicOutline = "Hierarchy Designer Folder Icon Classic Outline";
            internal const string FolderModernI = "Hierarchy Designer Folder Icon Modern I";
            internal const string FolderModernII = "Hierarchy Designer Folder Icon Modern II";
            internal const string FolderModernIII = "Hierarchy Designer Folder Icon Modern III";
            internal const string FolderModernOutline = "Hierarchy Designer Folder Icon Modern Outline";
            internal const string FolderNeoI = "Hierarchy Designer Folder Icon Neo I";
            internal const string FolderNeoII = "Hierarchy Designer Folder Icon Neo II";
            internal const string FolderNeoOutline = "Hierarchy Designer Folder Icon Neo Outline";
            internal const string SeparatorInspectorIcon = "Hierarchy Designer Separator Icon Inspector";
            internal const string SeparatorDefault = "Hierarchy Designer Separator Background Image Default";
            internal const string SeparatorDefaultFadedBottom = "Hierarchy Designer Separator Background Image Default Faded Bottom";
            internal const string SeparatorDefaultFadedLeft = "Hierarchy Designer Separator Background Image Default Faded Left";
            internal const string SeparatorDefaultFadedSideways = "Hierarchy Designer Separator Background Image Default Faded Sideways";
            internal const string SeparatorDefaultFadedRight = "Hierarchy Designer Separator Background Image Default Faded Right";
            internal const string SeparatorDefaultFadedTop = "Hierarchy Designer Separator Background Image Default Faded Top";
            internal const string SeparatorClassicI = "Hierarchy Designer Separator Background Image Classic I";
            internal const string SeparatorClassicII = "Hierarchy Designer Separator Background Image Classic II";
            internal const string SeparatorModernI = "Hierarchy Designer Separator Background Image Modern I";
            internal const string SeparatorModernII = "Hierarchy Designer Separator Background Image Modern II";
            internal const string SeparatorModernIII = "Hierarchy Designer Separator Background Image Modern III";
            internal const string SeparatorNeoI = "Hierarchy Designer Separator Background Image Neo I";
            internal const string SeparatorNeoII = "Hierarchy Designer Separator Background Image Neo II";
            internal const string SeparatorNextGenI = "Hierarchy Designer Separator Background Image Next-Gen I";
            internal const string SeparatorNextGenII = "Hierarchy Designer Separator Background Image Next-Gen II";
            internal const string SeparatorPostmodernI = "Hierarchy Designer Separator Background Image Postmodern I";
            internal const string SeparatorPostmodernII = "Hierarchy Designer Separator Background Image Postmodern II";
            #endregion

            #region Graphics
            internal const string GraphicsTitleDark = "Hierarchy Designer Graphics Title Dark";
            internal const string GraphicsTitleLight = "Hierarchy Designer Graphics Title Light";
            #endregion

            #region Promotional
            internal const string PromotionalPicEase = "Hierarchy Designer Promotional PicEase";
            internal const string PromotionalPicEaseLite = "Hierarchy Designer Promotional PicEase Lite";
            internal const string PromotionalPicEaseMini = "Hierarchy Designer Promotional PicEase Mini";
            internal const string PromotionalPicEasePostProcessingBuiltIn = "Hierarchy Designer Promotional PicEase Post Processing Built-In";
            internal const string PromotionalPicEasePostProcessingURP = "Hierarchy Designer Promotional PicEase Post Processing URP";
            internal const string PromotionalEditorFavorites = "Hierarchy Designer Promotional Editor Favorites";
            internal const string PromotionalMusicPlayerManager = "Hierarchy Designer Promotional Music Player Manager";
            internal const string PromotionalProceduralSkybox = "Hierarchy Designer Promotional Procedural Skybox 2.0";
            internal const string PromotionalProceduralSkyboxLite = "Hierarchy Designer Promotional Procedural Skybox 2.0 Lite";
            internal const string PromotionalProjectDesigner = "Hierarchy Designer Promotional Project Designer";
            internal const string PromotionalSeamlessGrassPatternTextures = "Hierarchy Designer Promotional Seamless Grass Pattern Textures";
            internal const string PromotionalSeamlessGrassTextures = "Hierarchy Designer Promotional Seamless Grass Textures";
            internal const string PromotionalSeamlessMudTextures = "Hierarchy Designer Promotional Seamless Mud Textures";
            internal const string PromotionalSeamlessSandTextures = "Hierarchy Designer Promotional Seamless Sand Textures";
            internal const string PromotionalSeamlessSnowTextures = "Hierarchy Designer Promotional Seamless Snow Textures";
            #endregion
        }

        #region Classes
        internal static class Fonts
        {
            private static readonly Lazy<Font> _bold = new(() => HD_Texture.LoadFont(ResourceNames.FontBold));
            public static Font Bold => _bold.Value;

            private static readonly Lazy<Font> _regular = new(() => HD_Texture.LoadFont(ResourceNames.FontRegular));
            public static Font Regular => _regular.Value;
        }

        internal static class Icons
        {
            private static readonly Lazy<Texture2D> _resetIcon = new(() => HD_Editor.IsProSkin ? HD_Texture.LoadTexture(ResourceNames.IconResetDark) : HD_Texture.LoadTexture(ResourceNames.IconResetLight));
            public static Texture2D Reset => _resetIcon.Value;

            private static readonly Lazy<Texture2D> _tooltipIcon = new(() => HD_Editor.IsProSkin ? HD_Texture.LoadTexture(ResourceNames.IconTooltipDark) : HD_Texture.LoadTexture(ResourceNames.IconTooltipLight));
            public static Texture2D Tooltip => _tooltipIcon.Value;

            private static readonly Lazy<Texture2D> _lockIcon = new(() => HD_Editor.IsProSkin ? HD_Texture.LoadTexture(ResourceNames.IconLockDark) : HD_Texture.LoadTexture(ResourceNames.IconLockLight));
            public static Texture2D Lock => _lockIcon.Value;

            private static readonly Lazy<Texture2D> _unlockIcon = new(() => HD_Editor.IsProSkin ? HD_Texture.LoadTexture(ResourceNames.IconUnlockDark) : HD_Texture.LoadTexture(ResourceNames.IconUnlockLight));
            public static Texture2D Unlock => _unlockIcon.Value;

            private static readonly Lazy<Texture2D> _visibilityOnIcon = new(() => HD_Editor.IsProSkin ? HD_Texture.LoadTexture(ResourceNames.IconVisibilityOnDark) : HD_Texture.LoadTexture(ResourceNames.IconVisibilityOnLight));
            public static Texture2D VisibilityOn => _visibilityOnIcon.Value;

            private static readonly Lazy<Texture2D> _visibilityOffIcon = new(() => HD_Editor.IsProSkin ? HD_Texture.LoadTexture(ResourceNames.IconVisibilityOffDark) : HD_Texture.LoadTexture(ResourceNames.IconVisibilityOffLight));
            public static Texture2D VisibilityOff => _visibilityOffIcon.Value;
        }

        internal static class Textures
        {
            private static readonly Lazy<Texture2D> _defaultTexture = new(() => HD_Texture.LoadTexture(ResourceNames.TextureDefault));
            public static Texture2D DefaultTexture => _defaultTexture.Value;

            #region Tree
            private static readonly Lazy<Texture2D> _treeBranchIDefault = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchIDefault));
            public static Texture2D TreeBranchIDefault => _treeBranchIDefault.Value;

            private static readonly Lazy<Texture2D> _treeBranchLDefault = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchLDefault));
            public static Texture2D TreeBranchLDefault => _treeBranchLDefault.Value;

            private static readonly Lazy<Texture2D> _treeBranchTDefault = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchTDefault));
            public static Texture2D TreeBranchTDefault => _treeBranchTDefault.Value;

            private static readonly Lazy<Texture2D> _treeBranchTerminalBudDefault = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchTerminalBudDefault));
            public static Texture2D TreeBranchTerminalBudDefault => _treeBranchTerminalBudDefault.Value;

            private static readonly Lazy<Texture2D> _treeBranchICurved = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchICurved));
            public static Texture2D TreeBranchICurved => _treeBranchICurved.Value;

            private static readonly Lazy<Texture2D> _treeBranchLCurved = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchLCurved));
            public static Texture2D TreeBranchLCurved => _treeBranchLCurved.Value;

            private static readonly Lazy<Texture2D> _treeBranchTCurved = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchTCurved));
            public static Texture2D TreeBranchTCurved => _treeBranchTCurved.Value;

            private static readonly Lazy<Texture2D> _treeBranchTerminalBudCurved = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchTerminalBudCurved));
            public static Texture2D TreeBranchTerminalBudCurved => _treeBranchTerminalBudCurved.Value;

            private static readonly Lazy<Texture2D> _treeBranchIDotted = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchIDotted));
            public static Texture2D TreeBranchIDotted => _treeBranchIDotted.Value;

            private static readonly Lazy<Texture2D> _treeBranchLDotted = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchLDotted));
            public static Texture2D TreeBranchLDotted => _treeBranchLDotted.Value;

            private static readonly Lazy<Texture2D> _treeBranchTDotted = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchTDotted));
            public static Texture2D TreeBranchTDotted => _treeBranchTDotted.Value;

            private static readonly Lazy<Texture2D> _treeBranchTerminalBudDotted = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchTerminalBudDotted));
            public static Texture2D TreeBranchTerminalBudDotted => _treeBranchTerminalBudDotted.Value;

            private static readonly Lazy<Texture2D> _treeBranchISegmented = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchISegmented));
            public static Texture2D TreeBranchISegmented => _treeBranchISegmented.Value;

            private static readonly Lazy<Texture2D> _treeBranchLSegmented = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchLSegmented));
            public static Texture2D TreeBranchLSegmented => _treeBranchLSegmented.Value;

            private static readonly Lazy<Texture2D> _treeBranchTSegmented = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchTSegmented));
            public static Texture2D TreeBranchTSegmented => _treeBranchTSegmented.Value;

            private static readonly Lazy<Texture2D> _treeBranchTerminalBudSegmented = new(() => HD_Texture.LoadTexture(ResourceNames.TreeBranchTerminalBudSegmented));
            public static Texture2D TreeBranchTerminalBudSegmented => _treeBranchTerminalBudSegmented.Value;
            #endregion

            #region Folder
            private static readonly Lazy<Texture2D> _folderScene = new(() => HD_Texture.LoadTexture(ResourceNames.FolderScene));
            public static Texture2D FolderScene => _folderScene.Value;

            private static readonly Lazy<Texture2D> _folderDefault = new(() => HD_Texture.LoadTexture(ResourceNames.FolderDefault));
            public static Texture2D FolderDefault => _folderDefault.Value;

            private static readonly Lazy<Texture2D> _folderDefaultOutline = new(() => HD_Texture.LoadTexture(ResourceNames.FolderDefaultOutline));
            public static Texture2D FolderDefaultOutline => _folderDefaultOutline.Value;

            private static readonly Lazy<Texture2D> _folderClassicI = new(() => HD_Texture.LoadTexture(ResourceNames.FolderClassicI));
            public static Texture2D FolderClassicI => _folderClassicI.Value;

            private static readonly Lazy<Texture2D> _folderClassicII = new(() => HD_Texture.LoadTexture(ResourceNames.FolderClassicII));
            public static Texture2D FolderClassicII => _folderClassicII.Value;

            private static readonly Lazy<Texture2D> _folderClassicOutline = new(() => HD_Texture.LoadTexture(ResourceNames.FolderClassicOutline));
            public static Texture2D FolderClassicOutline => _folderClassicOutline.Value;

            private static readonly Lazy<Texture2D> _folderModernI = new(() => HD_Texture.LoadTexture(ResourceNames.FolderModernI));
            public static Texture2D FolderModernI => _folderModernI.Value;

            private static readonly Lazy<Texture2D> _folderModernII = new(() => HD_Texture.LoadTexture(ResourceNames.FolderModernII));
            public static Texture2D FolderModernII => _folderModernII.Value;

            private static readonly Lazy<Texture2D> _folderModernIII = new(() => HD_Texture.LoadTexture(ResourceNames.FolderModernIII));
            public static Texture2D FolderModernIII => _folderModernIII.Value;

            private static readonly Lazy<Texture2D> _folderModernOutline = new(() => HD_Texture.LoadTexture(ResourceNames.FolderModernOutline));
            public static Texture2D FolderModernOutline => _folderModernOutline.Value;

            private static readonly Lazy<Texture2D> _folderNeoI = new(() => HD_Texture.LoadTexture(ResourceNames.FolderNeoI));
            public static Texture2D FolderNeoI => _folderNeoI.Value;

            private static readonly Lazy<Texture2D> _folderNeoII = new(() => HD_Texture.LoadTexture(ResourceNames.FolderNeoII));
            public static Texture2D FolderNeoII => _folderNeoII.Value;

            private static readonly Lazy<Texture2D> _folderNeoOutline = new(() => HD_Texture.LoadTexture(ResourceNames.FolderNeoOutline));
            public static Texture2D FolderNeoOutline => _folderNeoOutline.Value;
            #endregion

            #region Separator
            private static readonly Lazy<Texture2D> _separatorInspectorIcon = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorInspectorIcon));
            public static Texture2D SeparatorInspectorIcon => _separatorInspectorIcon.Value;

            private static readonly Lazy<Texture2D> _separatorDefault = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorDefault));
            public static Texture2D SeparatorDefault => _separatorDefault.Value;

            private static readonly Lazy<Texture2D> _separatorDefaultFadedBottom = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorDefaultFadedBottom));
            public static Texture2D SeparatorDefaultFadedBottom => _separatorDefaultFadedBottom.Value;

            private static readonly Lazy<Texture2D> _separatorDefaultFadedLeft = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorDefaultFadedLeft));
            public static Texture2D SeparatorDefaultFadedLeft => _separatorDefaultFadedLeft.Value;

            private static readonly Lazy<Texture2D> _separatorDefaultFadedSideways = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorDefaultFadedSideways));
            public static Texture2D SeparatorDefaultFadedSideways => _separatorDefaultFadedSideways.Value;

            private static readonly Lazy<Texture2D> _separatorDefaultFadedRight = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorDefaultFadedRight));
            public static Texture2D SeparatorDefaultFadedRight => _separatorDefaultFadedRight.Value;

            private static readonly Lazy<Texture2D> _separatorDefaultFadedTop = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorDefaultFadedTop));
            public static Texture2D SeparatorDefaultFadedTop => _separatorDefaultFadedTop.Value;

            private static readonly Lazy<Texture2D> _separatorClassicI = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorClassicI));
            public static Texture2D SeparatorClassicI => _separatorClassicI.Value;

            private static readonly Lazy<Texture2D> _separatorClassicII = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorClassicII));
            public static Texture2D SeparatorClassicII => _separatorClassicII.Value;

            private static readonly Lazy<Texture2D> _separatorModernI = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorModernI));
            public static Texture2D SeparatorModernI => _separatorModernI.Value;

            private static readonly Lazy<Texture2D> _separatorModernII = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorModernII));
            public static Texture2D SeparatorModernII => _separatorModernII.Value;

            private static readonly Lazy<Texture2D> _separatorModernIII = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorModernIII));
            public static Texture2D SeparatorModernIII => _separatorModernIII.Value;

            private static readonly Lazy<Texture2D> _separatorNeoI = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorNeoI));
            public static Texture2D SeparatorNeoI => _separatorNeoI.Value;

            private static readonly Lazy<Texture2D> _separatorNeoII = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorNeoII));
            public static Texture2D SeparatorNeoII => _separatorNeoII.Value;

            private static readonly Lazy<Texture2D> _separatorNextGenI = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorNextGenI));
            public static Texture2D SeparatorNextGenI => _separatorNextGenI.Value;

            private static readonly Lazy<Texture2D> _separatorNextGenII = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorNextGenII));
            public static Texture2D SeparatorNextGenII => _separatorNextGenII.Value;

            private static readonly Lazy<Texture2D> _separatorPostmodernI = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorPostmodernI));
            public static Texture2D SeparatorPostmodernI => _separatorPostmodernI.Value;

            private static readonly Lazy<Texture2D> _separatorPostmodernII = new(() => HD_Texture.LoadTexture(ResourceNames.SeparatorPostmodernII));
            public static Texture2D SeparatorPostmodernII => _separatorPostmodernII.Value;
            #endregion
        }

        internal static class Graphics
        {
            private static readonly Lazy<Texture2D> _titleDark = new(() => HD_Texture.LoadTexture(ResourceNames.GraphicsTitleDark));
            public static Texture2D TitleDark => _titleDark.Value;

            private static readonly Lazy<Texture2D> _titleLight = new(() => HD_Texture.LoadTexture(ResourceNames.GraphicsTitleLight));
            public static Texture2D TitleLight => _titleLight.Value;
        }

        internal static class Promotional
        {
            private static readonly Lazy<Texture2D> _picEasePromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalPicEase));
            public static Texture2D PicEasePromotionalIcon => _picEasePromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _picEaseLitePromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalPicEaseLite));
            public static Texture2D PicEaseLitePromotionalIcon => _picEaseLitePromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _picEaseMiniPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalPicEaseMini));
            public static Texture2D PicEaseMiniPromotionalIcon => _picEaseMiniPromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _picEasePostProcessingBuiltInPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalPicEasePostProcessingBuiltIn));
            public static Texture2D PicEasePostProcessingBuiltInPromotionalIcon => _picEasePostProcessingBuiltInPromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _picEasePostProcessingURPPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalPicEasePostProcessingURP));
            public static Texture2D PicEasePostProcessingURPPromotionalIcon => _picEasePostProcessingURPPromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _editorFavoritesPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalEditorFavorites));
            public static Texture2D EditorFavoritesPromotionalIcon => _editorFavoritesPromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _musicPlayerManagerPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalMusicPlayerManager));
            public static Texture2D MusicPlayerManagerPromotionalIcon => _musicPlayerManagerPromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _proceduralSkyboxIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalProceduralSkybox));
            public static Texture2D ProceduralSkyboxIcon => _proceduralSkyboxIcon.Value;

            private static readonly Lazy<Texture2D> _proceduralSkyboxLiteIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalProceduralSkyboxLite));
            public static Texture2D ProceduralSkyboxLiteIcon => _proceduralSkyboxLiteIcon.Value;

            private static readonly Lazy<Texture2D> _projectDesignerIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalProjectDesigner));
            public static Texture2D ProjectDesignerIcon => _projectDesignerIcon.Value;

            private static readonly Lazy<Texture2D> _seamlessGrassPatternTexturesPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalSeamlessGrassPatternTextures));
            public static Texture2D SeamlessGrassPatternTexturesPromotionalIcon => _seamlessGrassPatternTexturesPromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _seamlessGrassTexturesPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalSeamlessGrassTextures));
            public static Texture2D SeamlessGrassTexturesPromotionalIcon => _seamlessGrassTexturesPromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _seamlessMudTexturesPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalSeamlessMudTextures));
            public static Texture2D SeamlessMudTexturesPromotionalIcon => _seamlessMudTexturesPromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _seamlessSandTexturesPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalSeamlessSandTextures));
            public static Texture2D SeamlessSandTexturesPromotionalIcon => _seamlessSandTexturesPromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _seamlessSnowTexturesPromotionalIcon = new(() => HD_Texture.LoadTexture(ResourceNames.PromotionalSeamlessSnowTextures));
            public static Texture2D SeamlessSnowTexturesPromotionalIcon => _seamlessSnowTexturesPromotionalIcon.Value;
        }
        #endregion

        #region Accessors
        public static Texture2D GetTreeBranchImageTypeI(HD_Settings.TreeBranchImageType imageType)
        {
            return imageType switch
            {
                HD_Settings.TreeBranchImageType.Curved => Textures.TreeBranchICurved,
                HD_Settings.TreeBranchImageType.Dotted => Textures.TreeBranchIDotted,
                HD_Settings.TreeBranchImageType.Segmented => Textures.TreeBranchISegmented,
                _ => Textures.TreeBranchIDefault,
            };
        }

        public static Texture2D GetTreeBranchImageTypeL(HD_Settings.TreeBranchImageType imageType)
        {
            return imageType switch
            {
                HD_Settings.TreeBranchImageType.Curved => Textures.TreeBranchLCurved,
                HD_Settings.TreeBranchImageType.Dotted => Textures.TreeBranchLDotted,
                HD_Settings.TreeBranchImageType.Segmented => Textures.TreeBranchLSegmented,
                _ => Textures.TreeBranchLDefault,
            };
        }

        public static Texture2D GetTreeBranchImageTypeT(HD_Settings.TreeBranchImageType imageType)
        {
            return imageType switch
            {
                HD_Settings.TreeBranchImageType.Curved => Textures.TreeBranchTCurved,
                HD_Settings.TreeBranchImageType.Dotted => Textures.TreeBranchTDotted,
                HD_Settings.TreeBranchImageType.Segmented => Textures.TreeBranchTSegmented,
                _ => Textures.TreeBranchTDefault,
            };
        }

        public static Texture2D GetTreeBranchImageTypeTerminalBud(HD_Settings.TreeBranchImageType imageType)
        {
            return imageType switch
            {
                HD_Settings.TreeBranchImageType.Curved => Textures.TreeBranchTerminalBudCurved,
                HD_Settings.TreeBranchImageType.Dotted => Textures.TreeBranchTerminalBudDotted,
                HD_Settings.TreeBranchImageType.Segmented => Textures.TreeBranchTerminalBudSegmented,
                _ => Textures.TreeBranchTerminalBudDefault,
            };
        }

        public static Texture2D GetFolderImageType(HD_Folders.FolderImageType folderImageType)
        {
            return folderImageType switch
            {
                HD_Folders.FolderImageType.DefaultOutline => Textures.FolderDefaultOutline,
                HD_Folders.FolderImageType.ClassicI => Textures.FolderClassicI,
                HD_Folders.FolderImageType.ClassicII => Textures.FolderClassicII,
                HD_Folders.FolderImageType.ClassicOutline => Textures.FolderClassicOutline,
                HD_Folders.FolderImageType.ModernI => Textures.FolderModernI,
                HD_Folders.FolderImageType.ModernII => Textures.FolderModernII,
                HD_Folders.FolderImageType.ModernIII => Textures.FolderModernIII,
                HD_Folders.FolderImageType.ModernOutline => Textures.FolderModernOutline,
                HD_Folders.FolderImageType.NeoI => Textures.FolderNeoI,
                HD_Folders.FolderImageType.NeoII => Textures.FolderNeoII,
                HD_Folders.FolderImageType.NeoOutline => Textures.FolderNeoOutline,
                _ => Textures.FolderDefault,
            };
        }

        public static Texture2D GetSeparatorImageType(HD_Separators.SeparatorImageType separatorImageType)
        {
            return separatorImageType switch
            {
                HD_Separators.SeparatorImageType.DefaultFadedBottom => Textures.SeparatorDefaultFadedBottom,
                HD_Separators.SeparatorImageType.DefaultFadedLeft => Textures.SeparatorDefaultFadedLeft,
                HD_Separators.SeparatorImageType.DefaultFadedSideways => Textures.SeparatorDefaultFadedSideways,
                HD_Separators.SeparatorImageType.DefaultFadedRight => Textures.SeparatorDefaultFadedRight,
                HD_Separators.SeparatorImageType.DefaultFadedTop => Textures.SeparatorDefaultFadedTop,
                HD_Separators.SeparatorImageType.ClassicI => Textures.SeparatorClassicI,
                HD_Separators.SeparatorImageType.ClassicII => Textures.SeparatorClassicII,
                HD_Separators.SeparatorImageType.ModernI => Textures.SeparatorModernI,
                HD_Separators.SeparatorImageType.ModernII => Textures.SeparatorModernII,
                HD_Separators.SeparatorImageType.ModernIII => Textures.SeparatorModernIII,
                HD_Separators.SeparatorImageType.NeoI => Textures.SeparatorNeoI,
                HD_Separators.SeparatorImageType.NeoII => Textures.SeparatorNeoII,
                HD_Separators.SeparatorImageType.NextGenI => Textures.SeparatorNextGenI,
                HD_Separators.SeparatorImageType.NextGenII => Textures.SeparatorNextGenII,
                HD_Separators.SeparatorImageType.PostmodernI => Textures.SeparatorPostmodernI,
                HD_Separators.SeparatorImageType.PostmodernII => Textures.SeparatorPostmodernII,
                _ => Textures.SeparatorDefault,
            };
        }
        #endregion
    }
}
#endif