// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Animancer.Editor.Tools
{
    /// <summary>[Editor-Only] [Pro-Only] A <see cref="SpriteModifierTool"/> for bulk-renaming <see cref="Sprite"/>s.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/tools/rename-sprites">
    /// Rename Sprites</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Tools/RenameSpritesTool
    /// 
    [Serializable]
    public class RenameSpritesTool : SpriteModifierTool
    {
        /************************************************************************************************************************/

        [NonSerialized] private List<string> _GeneratedNames;
        [NonSerialized] private bool _NamesAreDirty;
        [NonSerialized] private ReorderableList _SpritesDisplay;

        [SerializeField] private List<string> _ManualNames;
        [SerializeField] private int _FirstIndex = 1;
        [SerializeField] private int _MinimumDigits = 1;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int DisplayOrder => 2;

        /// <inheritdoc/>
        public override string Name => "Rename Sprites";

        /// <inheritdoc/>
        public override string HelpURL => Strings.DocsURLs.RenameSprites;

        /// <inheritdoc/>
        public override string Instructions
        {
            get
            {
                if (Sprites.Count == 0)
                    return "Select the Sprites you want to rename.";

                return "Enter the new name(s) you want to give the Sprites then click Apply." +
                    "\n\nEach Sprite below the name you enter will be given the same name" +
                    " until the next name which will restart the counter.";
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnEnable(int index)
        {
            base.OnEnable(index);

            _ManualNames ??= new();
            _GeneratedNames ??= new();

            _SpritesDisplay = AnimancerToolsWindow.CreateReorderableObjectList(Sprites, "Sprites to Rename");
            _SpritesDisplay.onChangedCallback += list => DirtyNames();
            _SpritesDisplay.drawElementCallback = DrawItem;
            _SpritesDisplay.elementHeight = AnimancerGUI.LineHeight * 3 + AnimancerGUI.StandardSpacing * 2;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnSelectionChanged()
        {
            base.OnSelectionChanged();
            DirtyNames();
        }

        /************************************************************************************************************************/

        /// <summary>Refreshes the <see cref="_GeneratedNames"/>.</summary>
        private void UpdateNames()
        {
            if (!_NamesAreDirty)
                return;

            _NamesAreDirty = false;

            var sprites = Sprites;

            AnimancerEditorUtilities.SetCount(_ManualNames, sprites.Count);
            AnimancerEditorUtilities.SetCount(_GeneratedNames, sprites.Count);

            string name = null;
            string digitFormat = null;
            int index = 0;
            for (int i = 0; i < sprites.Count; i++)
            {
                var newName = _ManualNames[i];
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    name = newName;
                    index = 0;

                    var nextNameIndex = IndexOfNextManualName(i);

                    var digits = Mathf.FloorToInt(Mathf.Log10(nextNameIndex - i)) + 1;
                    if (digits < _MinimumDigits)
                        digits = _MinimumDigits;

                    var formatCharacters = new char[digits];
                    for (int iDigit = 0; iDigit < digits; iDigit++)
                        formatCharacters[iDigit] = '0';
                    digitFormat = new string(formatCharacters);
                }

                _GeneratedNames[i] = string.IsNullOrWhiteSpace(name)
                    ? sprites[i].name
                    : name + (index + _FirstIndex).ToString(digitFormat);

                index++;
            }
        }

        /************************************************************************************************************************/

        private int IndexOfNextManualName(int startIndex)
        {
            for (int i = startIndex + 1; i < _ManualNames.Count; i++)
                if (!string.IsNullOrWhiteSpace(_ManualNames[i]))
                    return i;

            return _ManualNames.Count;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoBodyGUI()
        {
            base.DoBodyGUI();

#if ! UNITY_2D_SPRITE
            EditorGUILayout.HelpBox(
                "Without the 2D Sprite Package," +
                " any references to the renamed sprites will be lost (including animations).",
                MessageType.Warning);
#endif

            AnimancerToolsWindow.BeginChangeCheck();
            var firstIndex = EditorGUILayout.IntField("First Index", _FirstIndex);
            if (AnimancerToolsWindow.EndChangeCheck(ref _FirstIndex, Mathf.Max(firstIndex, 0)))
                DirtyNames();

            AnimancerToolsWindow.BeginChangeCheck();
            var digits = EditorGUILayout.IntField("Minimum Digits", _MinimumDigits);
            if (AnimancerToolsWindow.EndChangeCheck(ref _MinimumDigits, Mathf.Max(digits, 1)))
                DirtyNames();

            UpdateNames();

            _SpritesDisplay.DoLayoutList();

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                GUI.enabled = HasAnyNames();

                if (GUILayout.Button("Clear"))
                {
                    AnimancerGUI.Deselect();
                    AnimancerToolsWindow.RecordUndo();
                    _ManualNames.Clear();
                    DirtyNames();
                }

                GUI.enabled = HasAnyDifferentNames();

                if (GUILayout.Button("Apply"))
                {
                    AnimancerGUI.Deselect();
                    AskAndApply();
                }
            }
            GUILayout.EndHorizontal();
        }

        /************************************************************************************************************************/

        private void DrawItem(Rect area, int index, bool isActive, bool isFocused)
        {
            var sprites = Sprites;
            var sprite = sprites[index];

            var thumbnailWidth = Math.Min(area.height, area.width * 0.5f);
            var thumbnailArea = AnimancerGUI.StealFromLeft(ref area, thumbnailWidth, AnimancerGUI.StandardSpacing);

            AnimancerGUI.DrawSprite(thumbnailArea, sprite);

            area.y += (AnimancerGUI.LineHeight + AnimancerGUI.StandardSpacing) * 0.5f;
            area.height = AnimancerGUI.LineHeight;

            sprites[index] = DrawSpriteField(area, sprite);

            AnimancerGUI.NextVerticalArea(ref area);

            DrawName(area, index);
        }

        /************************************************************************************************************************/

        private Sprite DrawSpriteField(Rect area, Sprite sprite)
            => AnimancerGUI.DoObjectFieldGUI(area, "", sprite, false);

        /************************************************************************************************************************/

        private static GUIStyle _TextFieldStyle;

        private void DrawName(Rect area, int index)
        {
            area.y += 1;
            area.height = AnimancerGUI.LineHeight;

            var manualName = _ManualNames[index];
            var generatedName = _GeneratedNames[index];

            if (Event.current.type == EventType.Repaint)
            {
                _TextFieldStyle ??= new(EditorStyles.textField);

                _TextFieldStyle.fontStyle = string.IsNullOrWhiteSpace(manualName)
                    ? FontStyle.Italic
                    : FontStyle.Bold;

                GUI.TextField(area, generatedName, _TextFieldStyle);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                _ManualNames[index] = GUI.TextField(area, manualName);
                if (EditorGUI.EndChangeCheck())
                    DirtyNames();
            }
        }

        /************************************************************************************************************************/

        private bool HasAnyNames()
        {
            var sprites = Sprites;

            for (int i = 0; i < sprites.Count; i++)
                if (!string.IsNullOrWhiteSpace(_ManualNames[i]))
                    return true;

            return false;
        }

        private bool HasAnyDifferentNames()
        {
            var sprites = Sprites;

            for (int i = 0; i < sprites.Count; i++)
                if (sprites[i].name != _GeneratedNames[i])
                    return true;

            return false;
        }

        /************************************************************************************************************************/

        private void DirtyNames()
            => _NamesAreDirty = true;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override string AreYouSure =>
            "Are you sure you want to rename these Sprites?"
#if UNITY_2D_SPRITE
            ;
#else
            + "\n\nAny references to the renamed Sprites will be lost (including animations that use them)." 
            + " This can be avoided by importing Unity's 2D Sprite Package before using this tool.";
#endif

        /************************************************************************************************************************/

        private static Dictionary<Sprite, string> _SpriteToName;

        /// <inheritdoc/>
        protected override void BeforeApply()
        {
            if (_SpriteToName == null)
                _SpriteToName = new();
            else
                _SpriteToName.Clear();

            var sprites = Sprites;
            for (int i = 0; i < sprites.Count; i++)
            {
                _SpriteToName.Add(sprites[i], _GeneratedNames[i]);
            }

            // Renaming selected Sprites will lose the selection without triggering OnSelectionChanged.
            EditorApplication.delayCall += OnSelectionChanged;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void Modify(SpriteDataEditor data, int index, Sprite sprite)
        {
            data.SetName(index, _SpriteToName[sprite]);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void Modify(TextureImporter importer, List<Sprite> sprites)
        {
            if (sprites.Count == 1 && importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                var sprite = sprites[0];
                var fileName = Path.GetFileNameWithoutExtension(importer.assetPath);
                if (fileName == sprite.name)
                {
                    AssetDatabase.RenameAsset(importer.assetPath, _SpriteToName[sprite]);
                    sprites.Clear();
                }
            }

            base.Modify(importer, sprites);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AfterApply()
        {
            base.AfterApply();

            AnimancerToolsWindow.RecordUndo();
            _ManualNames.Clear();
        }

        /************************************************************************************************************************/
    }
}

#endif

