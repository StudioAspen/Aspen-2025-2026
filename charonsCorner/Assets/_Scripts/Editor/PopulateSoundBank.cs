using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Animancer;
using CharonsCorner.Runtime;
using System.IO;
using System.Linq;

namespace CharonsCorner.Editor
{
    public class PopulateSoundBank : EditorWindow
    {
        private AudioBankSO _bank;
        private string _searchPath = "Assets/Audio/OneShotIds";

        [MenuItem("CharonsCorner/Populate Sound Bank")]
        public static void ShowWindow()
        {
            GetWindow<PopulateSoundBank>("Populate Sound Bank");
        }

        private void OnGUI()
        {
            GUILayout.Label("Populate Sound Bank", EditorStyles.boldLabel);
            _bank = (AudioBankSO)EditorGUILayout.ObjectField("Sound Bank", _bank, typeof(AudioBankSO), false);
            _searchPath = EditorGUILayout.TextField("Search Path", _searchPath);

            if (GUILayout.Button("Populate"))
            {
                if (_bank == null)
                {
                    Debug.LogError("Please assign a Sound Bank.");
                    return;
                }

                Populate();
            }
        }

        private void Populate()
        {
            Debug.Log($"[PopulateSoundBank] Starting population using path: {_searchPath}");
            
            // Find all assets in the search path
            string[] guids = AssetDatabase.FindAssets("", new[] { _searchPath });
            Debug.Log($"[PopulateSoundBank] Found {guids.Length} total assets in folder.");
            
            List<StringAsset> stringAssets = new List<StringAsset>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                StringAsset asset = AssetDatabase.LoadAssetAtPath<StringAsset>(path);
                if (asset != null)
                {
                    stringAssets.Add(asset);
                }
            }
            Debug.Log($"[PopulateSoundBank] Successfully loaded {stringAssets.Count} StringAssets.");

            int addedCount = 0;
            
            // Cleanup null keys that might cause issues
            _bank.RemoveNullKeys();

            foreach (var asset in stringAssets)
            {
                if (_bank.Bank == null || !_bank.Bank.ContainsKey(asset))
                {
                    Debug.Log($"[PopulateSoundBank] Adding new entry for: {asset.name}");
                    _bank.AddEntry(asset, null);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                EditorUtility.SetDirty(_bank);
                AssetDatabase.SaveAssets();
                Debug.Log($"[PopulateSoundBank] Successfully added {addedCount} new entries to {_bank.name}.");
            }
            else
            {
                Debug.Log("[PopulateSoundBank] No new entries were found to add.");
            }
        }
    }
}
