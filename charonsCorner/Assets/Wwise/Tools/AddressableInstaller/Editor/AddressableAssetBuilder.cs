/*******************************************************************************
The content of this file includes portions of the proprietary AUDIOKINETIC Wwise
Technology released in source code form as part of the game integration package.
The content of this file may not be used without valid licenses to the
AUDIOKINETIC Wwise Technology.
Note that the use of the game engine is subject to the Unity(R) Terms of
Service at https://unity3d.com/legal/terms-of-service
 
License Usage
 
Licensees holding valid licenses to the AUDIOKINETIC Wwise Technology may use
this file in accordance with the end user license agreement provided with the
software or, alternatively, in accordance with the terms contained
in a written agreement between you and Audiokinetic Inc.
Copyright (c) 2025 Audiokinetic Inc.
*******************************************************************************/
#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
using System;
using System.IO;
using AK.Wwise.Unity.WwiseAddressables;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public static class AddressableAssetBuilder
{
    private static string Name = nameof(AddressableAssetBuilder);
    private static bool _useCustomBuildScript = false;
    private static string _addressableAssetBuilderPath = "";

    public static void ApplySettings(bool useCustomBuildScript, string addressableAssetBuilderPath)
    {
        _useCustomBuildScript = useCustomBuildScript;
        if (!useCustomBuildScript)
        {
            string folderPath = "Assets/AddressableAssetsData/DataBuilders";
            string assetPath = Path.Combine(folderPath, "BuildScriptWwisePacked.asset");
            AkWwiseEditorSettings.Instance.AddressableAssetBuilderPath = assetPath;
            AkWwiseEditorSettings.Instance.SaveSettings();
            _addressableAssetBuilderPath = assetPath;
        }
        else
        {
            _addressableAssetBuilderPath = addressableAssetBuilderPath;
        }
    }

    /// <summary>
    /// Execute an Addressable Group build using the Wwise Build script
    /// </summary>
    public static void Build()
    {
        // Get the Addressable Asset Settings
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            AddressableInstaller.LogError(Name,"AddressableAssetSettings not found. Ensure Addressables is set up in your project.");
            return;
        }

        // Find the custom build script
        BuildScriptPackedMode customBuildScript = AssetDatabase.LoadAssetAtPath<ScriptableObject>(_addressableAssetBuilderPath) as BuildScriptPackedMode;

        if (customBuildScript == null)
        {
            AddressableInstaller.LogError(Name,$"Asset at {_addressableAssetBuilderPath} could not be loaded or is not a BuildScriptPackedMode.");
        }

        // Set the custom build script as the active data builder
        settings.ActivePlayerDataBuilderIndex = settings.DataBuilders.IndexOf(customBuildScript);

        if (settings.ActivePlayerDataBuilderIndex < 0)
        {
            AddressableInstaller.LogError(Name,"Failed to set custom build script as active. Ensure it is added to Addressables settings.");
            return;
        }

        Debug.Log($"Using custom build script: {customBuildScript.Name}");
        // Start the Addressables build process
        UnityEngine.Debug.Log("Starting Addressables build...");
        AddressableAssetSettings.BuildPlayerContent();
        UnityEngine.Debug.Log("Addressables build completed successfully.");
    }
    /// <summary>
    /// Create the BuildScriptWwisePacked content builder.
    /// </summary>
    public static void CreateContentBuilder()
    {
        // Define the path where the asset will be created
        string folderPath = "Assets/AddressableAssetsData/DataBuilders";
        string assetPath = Path.Combine(folderPath, "BuildScriptWwisePacked.asset");

        // Ensure the folder exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            UnityEngine.Debug.Log($"Created folder: {folderPath}");
        }

        // Check if the asset already exists
        var existingAsset = AssetDatabase.LoadAssetAtPath<BuildScriptWwisePacked>(assetPath);
        if (existingAsset != null)
        {
            Debug.Log("BuildScriptWwisePacked.asset already exists.");
            return;
        }

        // Create and save the new asset
        var newAsset = ScriptableObject.CreateInstance<BuildScriptWwisePacked>();
        AssetDatabase.CreateAsset(newAsset, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log("Created BuildScriptWwisePacked.asset at " + assetPath);
    }
    
    /// <summary>
    /// Add the build script created at Assets/AddressableAssetsData/DataBuilders/BuildScriptWwisePacked.asset to the addressable group build
    /// </summary>
    public static void AddBuildScript()
    {
        // Path to the newly created build script asset
        string assetPath = "Assets/AddressableAssetsData/DataBuilders/BuildScriptWwisePacked.asset";

        // Load the Addressable Asset Settings
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            AddressableInstaller.LogError(Name,"AddressableAssetSettings not found. Ensure Addressables is set up in your project.");
            settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder, AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
        }

        // Load the custom build script asset
        var customBuildScript = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
        if (customBuildScript == null)
        {
            AddressableInstaller.LogError(Name,$"Build script asset not found at path: {assetPath}");
            return;
        }

        // Check if the build script is already added
        foreach (var builder in settings.DataBuilders)
        {
            if (builder == customBuildScript)
            {
                Debug.Log("Build script already exists in Addressables settings.");
                return;
            }
        }

        // Add the build script to the Addressables settings
        settings.DataBuilders.Add(customBuildScript);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log($"Added build script to Addressables settings: {assetPath}");
    }
    
    /// <summary>
    /// Utility function that combines all the necessary steps to do a Wwise Addressable Group build
    /// </summary>
    public static void AddressableSetup()
    {
        if (!_useCustomBuildScript)
        {
            CreateContentBuilder();
        }
        AddBuildScript();
        Build();
    }
}
#endif