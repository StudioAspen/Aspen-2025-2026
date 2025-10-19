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

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AddressableUpdater
{
    
#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
    [MenuItem("Wwise/Update Addressables")]
    public static void UpdateAddressables()
    {
        string wwiseVersion = AkUnitySoundEngine.WwiseVersion;
        string shortWwiseVersion = wwiseVersion.Substring(2, wwiseVersion.IndexOf("Build")-3); //-3 for the space and the 2 first character that are skipped.
        string manifestPath = "Packages/manifest.json";
        //If using local installation, we cannot update through the updater. The user have to update their package locally.
        string addressablesGitHubLink = "https://github.com/audiokinetic/WwiseUnityAddressables.git#";
        if (!File.Exists(manifestPath))
        {
            Debug.LogError($"Wwise Addressables Updater: {manifestPath} not found.");
            return;
        }
        try
        {
            List<string> lines = new List<string>();
            bool lineFoundAndReplaced = false;
            using (StreamReader reader = new StreamReader(manifestPath))
            {
                while (reader.ReadLine() is { } line)
                {
                    if (line.Contains(addressablesGitHubLink))
                    {
                        int hashIndex = line.IndexOf('#');
                        int quoteIndex = line.IndexOf('"', hashIndex);
                        if (hashIndex != -1) 
                        {
                            string prefix = line.Substring(0, hashIndex + 1);

                            string suffix = "";
                            if (quoteIndex != -1 && quoteIndex > hashIndex)
                            {
                                suffix = line.Substring(quoteIndex); 
                            }
                            else
                            {
                               
                                int commaIndex = line.LastIndexOf(',');
                                if (commaIndex != -1 && commaIndex > hashIndex)
                                {
                                    suffix = line.Substring(commaIndex);
                                }
                            }

                            // Reconstruct the line
                            string gitHubWwiseVersion = line.Substring(hashIndex+1, quoteIndex - hashIndex-1);
                            if (gitHubWwiseVersion != shortWwiseVersion)
                            {
                                line = prefix + 'v' +shortWwiseVersion + suffix;
                                lineFoundAndReplaced = true;
                            }
                            
                        }
                    }
                    lines.Add(line); 
                }
            }
            if (lineFoundAndReplaced)
            {
                File.WriteAllLines(manifestPath, lines);
                AssetDatabase.Refresh(); // Important to refresh Unity's asset database
                Debug.Log($"Wwise Addressables Updater: Successfully updated the Wwise Addressable Package to {shortWwiseVersion}.");
            }
            else
            {
                Debug.Log($"Wwise Addressables Updater: Already up to date. Current version {shortWwiseVersion}.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Wwise Addressables Updater: Error processing JSON file: {e.Message}");
        }
    }
#endif
}