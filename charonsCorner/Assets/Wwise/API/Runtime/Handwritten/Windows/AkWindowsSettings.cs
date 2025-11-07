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

﻿#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || UNITY_EDITOR_WIN
public partial class AkCommonUserSettings
{
	partial void SetSampleRate(AkPlatformInitSettings settings)
	{
		settings.uSampleRate = m_SampleRate;
	}
	
	protected partial string GetPluginPath()
	{
#if UNITY_EDITOR_WIN
		return System.IO.Path.GetFullPath(AkUtilities.GetPathInPackage(@"Runtime\Plugins\Windows\x86_64\DSP"));
#elif UNITY_STANDALONE_WIN
		string potentialPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Plugins" + System.IO.Path.DirectorySeparatorChar);
		string architectureName = "x86";
#if UNITY_64
		architectureName += "_64";
#endif
		if(System.IO.File.Exists(System.IO.Path.Combine(potentialPath, "AkUnitySoundEngine.dll")))
		{
			return potentialPath;
		}
		else if(System.IO.File.Exists(System.IO.Path.Combine(potentialPath, architectureName, "AkUnitySoundEngine.dll")))
		{
			return System.IO.Path.Combine(potentialPath, architectureName);
		}
		else
		{
			UnityEngine.Debug.Log("Cannot find Wwise plugin path");
			return null;
		}
#else
		return System.IO.Path.Combine(UnityEngine.Application.dataPath, "Plugins" + System.IO.Path.DirectorySeparatorChar);		
#endif
	}
}
#endif

public class AkWindowsSettings : AkWwiseInitializationSettings.PlatformSettings
{
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoadMethod]
	private static void AutomaticPlatformRegistration()
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}

		RegisterPlatformSettingsClass<AkWindowsSettings>("Windows");
	}
#endif // UNITY_EDITOR

	protected override AkCommonUserSettings GetUserSettings()
	{
		return UserSettings;
	}

	protected override AkCommonAdvancedSettings GetAdvancedSettings()
	{
		return AdvancedSettings;
	}

	protected override AkCommonCommSettings GetCommsSettings()
	{
		return CommsSettings;
	}

	[System.Serializable]
	public class PlatformAdvancedSettings : AkCommonAdvancedSettings
	{
		[UnityEngine.Tooltip("Maximum number of System Audio Objects to reserve. Other processes will not be able to use them. Default is 128.")]
		public uint MaxSystemAudioObjects = 128;

		public override void CopyTo(AkPlatformInitSettings settings)
		{
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || UNITY_EDITOR_WIN
			settings.uMaxSystemAudioObjects = MaxSystemAudioObjects;
#endif
		}
	}

	[UnityEngine.HideInInspector]
	public AkCommonUserSettings UserSettings;

	[UnityEngine.HideInInspector]
	public PlatformAdvancedSettings AdvancedSettings;

	[UnityEngine.HideInInspector]
	public AkCommonCommSettings CommsSettings;
}
