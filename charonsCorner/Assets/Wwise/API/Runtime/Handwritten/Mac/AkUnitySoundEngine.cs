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

﻿public partial class AkUnitySoundEngine
{
#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
	/// <summary>
	///     Converts "AkOSChar*" C-strings to C# strings.
	/// </summary>
	/// <param name="ptr">"AkOSChar*" memory pointer passed to C# as an IntPtr.</param>
	/// <returns>Converted string.</returns>
	public static string StringFromIntPtrOSString(System.IntPtr ptr)
	{
		return StringFromIntPtrString(ptr);
	}

	public static bool PlatformSupportsDecodeBank()
	{
		return true;
	}

	public static AKRESULT SeekOnEvent(uint in_eventID, UnityEngine.GameObject in_gameObjectID, float in_fPercent, bool in_bSeekToNearestMarker, uint in_PlayingID)
	{

		var in_gameObjectID_id = AkUnitySoundEngine.GetAkGameObjectID(in_gameObjectID);
		AkUnitySoundEngine.PreGameObjectAPICall(in_gameObjectID, in_gameObjectID_id);

        return (AKRESULT)AkUnitySoundEnginePINVOKE.CSharp_SeekOnEvent__SWIG_9(in_eventID, in_gameObjectID_id, in_fPercent, in_bSeekToNearestMarker, in_PlayingID);
	}

	public static AKRESULT SeekOnEvent(uint in_eventID, UnityEngine.GameObject in_gameObjectID, float in_fPercent, bool in_bSeekToNearestMarker)
	{
		var in_gameObjectID_id = AkUnitySoundEngine.GetAkGameObjectID(in_gameObjectID);
		AkUnitySoundEngine.PreGameObjectAPICall(in_gameObjectID, in_gameObjectID_id);

        return (AKRESULT)AkUnitySoundEnginePINVOKE.CSharp_SeekOnEvent__SWIG_10(in_eventID, in_gameObjectID_id, in_fPercent, in_bSeekToNearestMarker);
	}

	public static AKRESULT SeekOnEvent(uint in_eventID, UnityEngine.GameObject in_gameObjectID, float in_fPercent)
	{
		var in_gameObjectID_id = AkUnitySoundEngine.GetAkGameObjectID(in_gameObjectID);
		AkUnitySoundEngine.PreGameObjectAPICall(in_gameObjectID, in_gameObjectID_id);

        return (AKRESULT)AkUnitySoundEnginePINVOKE.CSharp_SeekOnEvent__SWIG_11(in_eventID, in_gameObjectID_id, in_fPercent);
	}

	public static AKRESULT SeekOnEvent(string in_pszEventName, UnityEngine.GameObject in_gameObjectID, float in_fPercent, bool in_bSeekToNearestMarker, uint in_PlayingID)
	{
		var in_gameObjectID_id = AkUnitySoundEngine.GetAkGameObjectID(in_gameObjectID);
		AkUnitySoundEngine.PreGameObjectAPICall(in_gameObjectID, in_gameObjectID_id);

        return (AKRESULT)AkUnitySoundEnginePINVOKE.CSharp_SeekOnEvent__SWIG_12(in_pszEventName, in_gameObjectID_id, in_fPercent, in_bSeekToNearestMarker, in_PlayingID);
	}

	public static AKRESULT SeekOnEvent(string in_pszEventName, UnityEngine.GameObject in_gameObjectID, float in_fPercent, bool in_bSeekToNearestMarker)
	{
		var in_gameObjectID_id = AkUnitySoundEngine.GetAkGameObjectID(in_gameObjectID);
		AkUnitySoundEngine.PreGameObjectAPICall(in_gameObjectID, in_gameObjectID_id);

        return (AKRESULT)AkUnitySoundEnginePINVOKE.CSharp_SeekOnEvent__SWIG_13(in_pszEventName, in_gameObjectID_id, in_fPercent, in_bSeekToNearestMarker);
	}

	public static AKRESULT SeekOnEvent(string in_pszEventName, UnityEngine.GameObject in_gameObjectID, float in_fPercent)
	{
		var in_gameObjectID_id = AkUnitySoundEngine.GetAkGameObjectID(in_gameObjectID);
		AkUnitySoundEngine.PreGameObjectAPICall(in_gameObjectID, in_gameObjectID_id);

        return (AKRESULT)AkUnitySoundEnginePINVOKE.CSharp_SeekOnEvent__SWIG_14(in_pszEventName, in_gameObjectID_id, in_fPercent);
	}
#endif
}
