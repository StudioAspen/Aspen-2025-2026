// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor.Tools
{
    /// <summary>[Editor-Only] [Pro-Only] 
    /// A <see cref="SpriteModifierTool"/> for generating <see cref="AnimationClip"/>s from <see cref="Sprite"/>s.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/tools/generate-sprite-animations">
    /// Generate Sprite Animations</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Tools/GenerateSpriteAnimationsTool
    /// 
    [Serializable]
    public class GenerateSpriteAnimationsTool : SpriteModifierTool
    {
        /************************************************************************************************************************/
        #region Tool
        /************************************************************************************************************************/

        [NonSerialized] private List<string> _Names;
        [NonSerialized] private Dictionary<string, List<Sprite>> _NameToSprites;
        [NonSerialized] private ReorderableList _Display;
        [NonSerialized] private bool _NamesAreDirty;
        [NonSerialized] private double _PreviewStartTime;
        [NonSerialized] private long _PreviewFrameIndex;
        [NonSerialized] private bool _RequiresRepaint;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int DisplayOrder => 3;

        /// <inheritdoc/>
        public override string Name => "Generate Sprite Animations";

        /// <inheritdoc/>
        public override string HelpURL => Strings.DocsURLs.GenerateSpriteAnimations;

        /// <inheritdoc/>
        public override string Instructions
        {
            get
            {
                if (Sprites.Count == 0)
                    return "Select the Sprites you want to generate animations from.";

                return "Configure the animation settings then click Generate.";
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnEnable(int index)
        {
            base.OnEnable(index);

            _Names = new();
            _NameToSprites = new();

            _Display = AnimancerToolsWindow.CreateReorderableList(
                _Names,
                "Animations to Generate",
                DrawDisplayElement);
            _Display.elementHeightCallback = CalculateDisplayElementHeight;

            _PreviewStartTime = EditorApplication.timeSinceStartup;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnSelectionChanged()
        {
            _NameToSprites.Clear();
            _Names.Clear();
            _NamesAreDirty = true;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoBodyGUI()
        {
            var property = GenerateSpriteAnimationsSettings.SerializedProperty;
            property.serializedObject.Update();
            using (var label = PooledGUIContent.Acquire("Settings"))
                EditorGUILayout.PropertyField(property, label, true);
            property.serializedObject.ApplyModifiedProperties();

            GenerateSpriteAnimationsSettings.Instance.FillDefaults();

            var sprites = Sprites;

            if (_NamesAreDirty)
            {
                _NamesAreDirty = false;
                GatherNameToSprites(sprites, _NameToSprites);
                _Names.AddRange(_NameToSprites.Keys);
            }

            using (new EditorGUI.DisabledScope(true))
            {
                var previewCurrentTime = EditorApplication.timeSinceStartup - _PreviewStartTime;
                _PreviewFrameIndex = (long)(previewCurrentTime * GenerateSpriteAnimationsSettings.FrameRate);

                _Display.DoLayoutList();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    GUI.enabled = sprites.Count > 0;

                    if (GUILayout.Button("Generate"))
                    {
                        Deselect();
                        GenerateAnimationsBySpriteName(sprites);
                    }
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.HelpBox("This function is also available via:" +
                "\n• The 'Assets/Create/Animancer' menu." +
                "\n• The Context Menu in the top right of the Inspector for Sprite and Texture assets",
                MessageType.Info);

            if (_RequiresRepaint)
            {
                _RequiresRepaint = false;
                AnimancerToolsWindow.Repaint();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the height of an animation to generate.</summary>
        private float CalculateDisplayElementHeight(int index)
        {
            if (_NameToSprites.Count <= 0 || _Names.Count <= 0)
                return 0;

            var lineCount = _NameToSprites[_Names[index]].Count + 3;
            return (LineHeight + StandardSpacing) * lineCount;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the details of an animation to generate.</summary>
        private void DrawDisplayElement(Rect area, int index, bool isActive, bool isFocused)
        {
            area.y = Mathf.Ceil(area.y + StandardSpacing * 0.5f);
            area.height = LineHeight;

            DrawAnimationHeader(ref area, index, out var sprites);
            DrawAnimationBody(ref area, sprites);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the name and preview of an animation to generate.</summary>
        private void DrawAnimationHeader(ref Rect area, int index, out List<Sprite> sprites)
        {
            var width = area.width;

            var previewSize = 3 * LineHeight + 2 * StandardSpacing;
            var previewArea = StealFromRight(ref area, previewSize, StandardSpacing);
            previewArea.height = previewSize;

            // Name.

            var name = _Names[index];

            AnimancerToolsWindow.BeginChangeCheck();
            name = EditorGUI.TextField(area, name);
            if (AnimancerToolsWindow.EndChangeCheck())
            {
                _Names[index] = name;
            }

            NextVerticalArea(ref area);

            // Frame Count.

            sprites = _NameToSprites[name];

            var frame = (int)(_PreviewFrameIndex % sprites.Count);

            var enabled = GUI.enabled;
            GUI.enabled = false;

            EditorGUI.TextField(area, $"Frame {frame} / {sprites.Count}");

            NextVerticalArea(ref area);

            // Preview Time.

            GUI.enabled = true;

            var beforeControlID = GUIUtility.GetControlID(FocusType.Passive);

            var newFrame = EditorGUI.IntSlider(area, frame, 0, sprites.Count);

            var afterControlID = GUIUtility.GetControlID(FocusType.Passive);
            var hotControl = GUIUtility.hotControl;

            if (newFrame != frame ||
                (hotControl > beforeControlID && hotControl < afterControlID))
            {
                _PreviewStartTime = EditorApplication.timeSinceStartup;
                _PreviewStartTime -= newFrame / GenerateSpriteAnimationsSettings.FrameRate;
                _PreviewFrameIndex = newFrame;
                frame = newFrame % sprites.Count;
            }

            GUI.enabled = enabled;

            NextVerticalArea(ref area);

            area.width = width;

            // Preview.

            DrawSprite(previewArea, sprites[frame]);
            _RequiresRepaint = true;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the sprite contents of an animation to generate.</summary>
        private void DrawAnimationBody(ref Rect area, List<Sprite> sprites)
        {
            var previewFrame = (int)(_PreviewFrameIndex % sprites.Count);

            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];

                var fieldArea = area;
                var thumbnailArea = StealFromLeft(
                    ref fieldArea,
                    fieldArea.height,
                    StandardSpacing);

                AnimancerToolsWindow.BeginChangeCheck();
                sprite = DoObjectFieldGUI(fieldArea, "", sprite, false);
                if (AnimancerToolsWindow.EndChangeCheck())
                {
                    sprites[i] = sprite;
                }

                if (i == previewFrame)
                    EditorGUI.DrawRect(fieldArea, new(0.25f, 1, 0.25f, 0.1f));

                DrawSprite(thumbnailArea, sprite);

                NextVerticalArea(ref area);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <summary>Uses <see cref="GatherNameToSprites"/> and creates new animations from those groups.</summary>
        private static void GenerateAnimationsBySpriteName(List<Sprite> sprites)
        {
            if (sprites.Count == 0)
                return;

            sprites.Sort(NaturalCompare);

            var nameToSprites = new Dictionary<string, List<Sprite>>();
            GatherNameToSprites(sprites, nameToSprites);

            var pathToSprites = new Dictionary<string, List<Sprite>>();

            var message = StringBuilderPool.Instance.Acquire()
                .Append("Do you wish to generate the following animations?");

            const int MaxLines = 25;
            var line = 0;
            foreach (var nameToSpriteGroup in nameToSprites)
            {
                var path = AssetDatabase.GetAssetPath(nameToSpriteGroup.Value[0]);
                path = Path.GetDirectoryName(path);
                path = Path.Combine(path, nameToSpriteGroup.Key + ".anim");
                pathToSprites.Add(path, nameToSpriteGroup.Value);

                if (++line <= MaxLines)
                {
                    message.AppendLine()
                        .Append("- ")
                        .Append(path)
                        .Append(" (")
                        .Append(nameToSpriteGroup.Value.Count)
                        .Append(" frames)");
                }
            }

            if (line > MaxLines)
            {
                message.AppendLine()
                    .Append("And ")
                    .Append(line - MaxLines)
                    .Append(" others.");
            }

            if (!EditorUtility.DisplayDialog("Generate Sprite Animations?", message.ReleaseToString(), "Generate", "Cancel"))
                return;

            foreach (var pathToSpriteGroup in pathToSprites)
                CreateAnimation(pathToSpriteGroup.Key, pathToSpriteGroup.Value.ToArray());

            AssetDatabase.SaveAssets();
        }

        /************************************************************************************************************************/

        private static char[] _Numbers, _TrimOther;

        /// <summary>Groups the `sprites` by name into the `nameToSptires`.</summary>
        private static void GatherNameToSprites(List<Sprite> sprites, Dictionary<string, List<Sprite>> nameToSprites)
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                var name = sprite.name;

                // Remove numbers from the end.
                _Numbers ??= new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                name = name.TrimEnd(_Numbers);

                // Then remove other characters from the end.
                _TrimOther ??= new char[] { ' ', '_', '-' };
                name = name.TrimEnd(_TrimOther);

                // Doing both at once would turn "Attack2-0" (Attack 2 Frame 0) into "Attack" (losing the number).

                if (!nameToSprites.TryGetValue(name, out var spriteGroup))
                {
                    spriteGroup = new();
                    nameToSprites.Add(name, spriteGroup);
                }

                // Add the sprite to the group if it's not a duplicate.
                if (spriteGroup.Count == 0 || spriteGroup[^1] != sprite)
                    spriteGroup.Add(sprite);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates and saves a new <see cref="AnimationClip"/> that plays the `sprites`.</summary>
        private static void CreateAnimation(string path, params Sprite[] sprites)
        {
            var frameRate = GenerateSpriteAnimationsSettings.FrameRate;
            var hierarchyPath = GenerateSpriteAnimationsSettings.HierarchyPath;
            var type = GenerateSpriteAnimationsSettings.TargetType.Type ?? typeof(SpriteRenderer);

            var property = GenerateSpriteAnimationsSettings.PropertyName;
            if (string.IsNullOrWhiteSpace(property))
                property = "m_Sprite";

            var clip = new AnimationClip
            {
                frameRate = frameRate,
            };

            var spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < spriteKeyFrames.Length; i++)
            {
                spriteKeyFrames[i] = new()
                {
                    time = i / (float)frameRate,
                    value = sprites[i]
                };
            }

            var spriteBinding = EditorCurveBinding.PPtrCurve(hierarchyPath, type, property);
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);

            AssetDatabase.CreateAsset(clip, path);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Menu Functions
        /************************************************************************************************************************/

        private const string GenerateAnimationsBySpriteNameFunctionName = "Generate Animations By Sprite Name";

        /************************************************************************************************************************/

        /// <summary>Should <see cref="GenerateAnimationsBySpriteName()"/> be enabled or greyed out?</summary>
        [MenuItem(Strings.CreateMenuPrefix + GenerateAnimationsBySpriteNameFunctionName, validate = true)]
        private static bool ValidateGenerateAnimationsBySpriteName()
        {
            var selection = Selection.objects;
            for (int i = 0; i < selection.Length; i++)
            {
                var selected = selection[i];
                if (selected is Sprite || selected is Texture)
                    return true;
            }

            return false;
        }

        /// <summary>Calls <see cref="GenerateAnimationsBySpriteName(List{Sprite})"/> with the selected <see cref="Sprite"/>s.</summary>
        [MenuItem(
            itemName: Strings.CreateMenuPrefix + GenerateAnimationsBySpriteNameFunctionName,
            priority = Strings.AssetMenuOrder + 8)]
        private static void GenerateAnimationsBySpriteName()
        {
            var sprites = new List<Sprite>();

            var selection = Selection.objects;
            for (int i = 0; i < selection.Length; i++)
            {
                var selected = selection[i];
                if (selected is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
                else if (selected is Texture2D texture)
                {
                    sprites.AddRange(LoadAllSpritesInTexture(texture));
                }
            }

            GenerateAnimationsBySpriteName(sprites);
        }

        /************************************************************************************************************************/

        private static List<Sprite> _CachedSprites;

        /// <summary>
        /// Returns a list of <see cref="Sprite"/>s which will be passed into
        /// <see cref="GenerateAnimationsBySpriteName(List{Sprite})"/> by <see cref="EditorApplication.delayCall"/>.
        /// </summary>
        private static List<Sprite> GetCachedSpritesToGenerateAnimations()
        {
            if (_CachedSprites == null)
                return _CachedSprites = new();

            // Delay the call in case multiple objects are selected.
            if (_CachedSprites.Count == 0)
            {
                EditorApplication.delayCall += () =>
                {
                    GenerateAnimationsBySpriteName(_CachedSprites);
                    _CachedSprites.Clear();
                };
            }

            return _CachedSprites;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Adds the <see cref="MenuCommand.context"/> to the <see cref="GetCachedSpritesToGenerateAnimations"/>.
        /// </summary>
        [MenuItem("CONTEXT/" + nameof(Sprite) + GenerateAnimationsBySpriteNameFunctionName)]
        private static void GenerateAnimationsFromSpriteByName(MenuCommand command)
        {
            GetCachedSpritesToGenerateAnimations().Add((Sprite)command.context);
        }

        /************************************************************************************************************************/

        /// <summary>Should <see cref="GenerateAnimationsFromTextureBySpriteName"/> be enabled or greyed out?</summary>
        [MenuItem("CONTEXT/" + nameof(TextureImporter) + GenerateAnimationsBySpriteNameFunctionName, validate = true)]
        private static bool ValidateGenerateAnimationsFromTextureBySpriteName(MenuCommand command)
        {
            var importer = (TextureImporter)command.context;
            var sprites = LoadAllSpritesAtPath(importer.assetPath);
            return sprites.Length > 0;
        }

        /// <summary>
        /// Adds all <see cref="Sprite"/> sub-assets of the <see cref="MenuCommand.context"/> to the
        /// <see cref="GetCachedSpritesToGenerateAnimations"/>.
        /// </summary>
        [MenuItem("CONTEXT/" + nameof(TextureImporter) + GenerateAnimationsBySpriteNameFunctionName)]
        private static void GenerateAnimationsFromTextureBySpriteName(MenuCommand command)
        {
            var cachedSprites = GetCachedSpritesToGenerateAnimations();
            var importer = (TextureImporter)command.context;
            cachedSprites.AddRange(LoadAllSpritesAtPath(importer.assetPath));
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
    #region Settings
    /************************************************************************************************************************/

    /// <summary>[Editor-Only] Settings for <see cref="GenerateSpriteAnimationsTool"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Tools/GenerateSpriteAnimationsSettings
    [Serializable, InternalSerializableType]
    public class GenerateSpriteAnimationsSettings : AnimancerSettingsGroup
    {
        /************************************************************************************************************************/

        /// <summary>Gets or creates an instance.</summary>
        public static GenerateSpriteAnimationsSettings Instance
            => AnimancerSettingsGroup<GenerateSpriteAnimationsSettings>.Instance;

        /// <summary>The <see cref="UnityEditor.SerializedProperty"/> representing the <see cref="Instance"/>.</summary>
        public static SerializedProperty SerializedProperty
            => Instance.GetSerializedProperty(null);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string DisplayName
            => "Generate Sprite Animations Tool";

        /// <inheritdoc/>
        public override int Index
            => 6;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("The frame rate to use for new animations")]
        private float _FrameRate = 12;

        /// <summary>The frame rate to use for new animations.</summary>
        public static ref float FrameRate
            => ref Instance._FrameRate;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("The Transform Hierarchy path from the Animator to the object being animated" +
            " using forward slashes '/' between each object name")]
        private string _HierarchyPath;

        /// <summary>The Transform Hierarchy path from the <see cref="Animator"/> to the object being animated.</summary>
        public static ref string HierarchyPath
            => ref Instance._HierarchyPath;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("The type of component being animated. Defaults to " + nameof(SpriteRenderer) + " if not set." +
            " Use the type picker on the right or drag and drop a component onto it to set this field.")]
        private SerializableTypeReference _TargetType = new(typeof(SpriteRenderer));

        /// <summary>The type of component being animated. Defaults to <see cref="SpriteRenderer"/> if not set.</summary>
        public static ref SerializableTypeReference TargetType
            => ref Instance._TargetType;

        /************************************************************************************************************************/

        /// <summary>The default value for <see cref="PropertyName"/>.</summary>
        public const string DefaultPropertyName = "m_Sprite";

        [SerializeField]
        [Tooltip("The path of the property being animated. Defaults to " + DefaultPropertyName + " if not set.")]
        private string _PropertyName = DefaultPropertyName;

        /// <summary>The path of the property being animated.</summary>
        public static ref string PropertyName
            => ref Instance._PropertyName;

        /************************************************************************************************************************/

        /// <summary>Reverts any empty values to their defaults.</summary>
        public void FillDefaults()
        {
            if (string.IsNullOrWhiteSpace(_TargetType.QualifiedName))
                _TargetType = new(typeof(SpriteRenderer));

            if (string.IsNullOrWhiteSpace(_PropertyName))
                _PropertyName = DefaultPropertyName;
        }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
    #endregion
    /************************************************************************************************************************/
}

#endif

