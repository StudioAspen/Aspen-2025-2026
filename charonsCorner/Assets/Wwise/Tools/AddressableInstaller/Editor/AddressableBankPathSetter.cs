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
using System.IO;
using UnityEditor;
using UnityEngine;

public class AddressableBankPathSetter
{
    /// <summary>
    /// Set the bank path for unity addressable
    /// </summary>
    /// <param name="bankFolder">The path of the folder in which to output the bank. Should be relative to the Asset Folder</param>
    public static bool SetBankPath(string bankFolder)
    {
	    if (bankFolder == null)
	    {
		    UnityEngine.Debug.LogError("AddressableBankPathSetter: Bank folder was not specified.");
		    return false;
	    }

	    var fullPath = Path.Combine(UnityEngine.Application.dataPath, bankFolder);
	    fullPath = fullPath.Replace('\\', '/');
	    
		if (bankFolder.Length != 0)
		{
			var settings = AkWwiseEditorSettings.Instance;
			settings.GeneratedSoundbanksPath = bankFolder;
			var projectPath = AkUtilities.GetFullPath(UnityEngine.Application.dataPath, settings.WwiseProjectPath);
			var relPath = AkUtilities.MakeRelativePath(System.IO.Path.GetDirectoryName(projectPath), fullPath);
			AkUtilities.SetSoundbanksDestinationFoldersInWproj(projectPath, relPath);
			settings.SaveSettings();
		}

		return true;
    }

    public static bool SetExternalSourcePath(string externalSourcePath)
    {
	    if (externalSourcePath == null)
	    {
		    UnityEngine.Debug.LogError("AddressableBankPathSetter: Bank folder was not specified.");
		    return false;
	    }

	    var fullPath = Path.Combine(UnityEngine.Application.dataPath, externalSourcePath);
	    fullPath = fullPath.Replace('\\', '/');
	    
	    if (externalSourcePath.Length != 0)
	    {
		    var settings = AkWwiseEditorSettings.Instance;
		    var projectPath = AkUtilities.GetFullPath(UnityEngine.Application.dataPath, settings.WwiseProjectPath);
		    var relPath = AkUtilities.MakeRelativePath(System.IO.Path.GetDirectoryName(projectPath), fullPath);
		    AkUtilities.SetExternalSourceDestinationFolderInWproj(projectPath, relPath);
		    settings.SaveSettings();
	    }

	    return true;
    }
    
    public static void SetSoundbankPath(string soundBankPath)
    {
	    if (soundBankPath == null)
	    {
		    UnityEngine.Debug.LogError("AddressableBankPathSetter: Bank folder was not specified.");
		    return;
	    }

	    var fullPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, soundBankPath);
	    fullPath = fullPath.Replace('\\', '/');
	    
	    if (soundBankPath.Length != 0)
	    {
		    var settings = AkWwiseEditorSettings.Instance;
		    settings.GeneratedSoundbanksPath = AkUtilities.MakeRelativePath(UnityEngine.Application.dataPath, fullPath);
		    var projectPath = AkUtilities.GetFullPath(UnityEngine.Application.dataPath, settings.WwiseProjectPath);
		    var relPath = AkUtilities.MakeRelativePath(System.IO.Path.GetDirectoryName(projectPath), fullPath);
		    AkUtilities.SetWwiseRootOutputPath(projectPath, relPath);
		    AkUtilities.SetPlatformsSoundBankPath(projectPath, "GeneratedSoundBanks");
		    settings.SaveSettings();
	    }
    }
}
