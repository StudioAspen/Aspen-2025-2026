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
#if UNITY_EDITOR_LINUX || (UNITY_STANDALONE_LINUX && !UNITY_EDITOR)
using System.Threading.Tasks;

public partial class WwiseProjectDatabase
{
    static WwiseProjectDatabase()
    {
        PlatformName = "Linux";
    }
    public static void PostInitCallback()
    {
        SoundBankDirectoryUpdated?.InvokeUnitySafe();
    }
    public static event System.Action SoundBankDirectoryUpdated;
    public static async Task<bool> InitAsync(string inDirectoryPath, string inDirectoryPlatformName)
    {
        InitCheckUp(inDirectoryPath);
        if (!ProjectInfoExists)
            return false;

        try
        {
            await Task.Run(() => WwiseProjectDatabasePINVOKE_Linux.Init(inDirectoryPath, inDirectoryPlatformName));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
        return ProjectInfoExists;
    }
    
    public static void Init(string inDirectoryPath, string inDirectoryPlatformName, string language = null)
    {
        InitCheckUp(inDirectoryPath);
        if (!ProjectInfoExists)
            return;

        try
        {
            WwiseProjectDatabasePINVOKE_Linux.Init(inDirectoryPath, inDirectoryPlatformName);
            if (!string.IsNullOrEmpty(language))
            {
                WwiseProjectDatabasePINVOKE_Linux.SetCurrentLanguage(language);
            }
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
      
    }
    
    public static void SetCurrentPlatform(string inDirectoryPlatformName)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Linux.SetCurrentPlatform(inDirectoryPlatformName);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static void SetCurrentLanguage(string inLanguageName)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Linux.SetCurrentLanguage(inLanguageName);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static string StringFromIntPtrString(System.IntPtr ptr)
    {
        try
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    /*
     * SoundBanks
     */

    public static global::System.IntPtr GetSoundBankRefString(string soundBankName, string soundBankType)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankRefString(soundBankName, soundBankType);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static global::System.IntPtr GetAllSoundBanksRef()
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetAllSoundBanksRef();
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static uint GetSoundBankCount()
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankCount();
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static global::System.IntPtr GetSoundBankRefIndex(global::System.IntPtr soundBankArrayRef, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankRefIndex(soundBankArrayRef, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static void DeleteSoundBanksArrayRef(global::System.IntPtr soundBankArrayRef)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Linux.DeleteSoundBanksArrayRef(soundBankArrayRef);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    public static string GetSoundBankName(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetSoundBankName(soundBankRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetSoundBankPath(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetSoundBankPath(soundBankRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetSoundBankLanguage(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetSoundBankLanguage(soundBankRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetSoundBankLanguageId(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankLanguageId(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static System.IntPtr GetSoundBankGuid(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankGuid(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetSoundBankShortId(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankShortId(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static bool IsUserBank(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.IsUserBank(soundBankRefPtr);

        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static bool IsInitBank(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.IsInitBank(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static bool IsSoundBankValid(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.IsSoundBankValid(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static global::System.IntPtr GetSoundBankMedia(global::System.IntPtr soundBankRefPtr, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankMedia(soundBankRefPtr, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static uint GetSoundBankMediasCount(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankMediasCount(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static global::System.IntPtr GetSoundBankEvent(global::System.IntPtr soundBankRefPtr, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankEvent(soundBankRefPtr, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static uint GetSoundBankEventsCount(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetSoundBankEventsCount(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static void DeleteSoundBankRef(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Linux.DeleteSoundBankRef(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    /*
     * Medias
    */
    
    public static string GetMediaName(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetMediaName(mediaRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetMediaPath(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetMediaPath(mediaRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetMediaShortId(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetMediaShortId(mediaRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetMediaLanguage(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetMediaLanguage(mediaRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static bool GetMediaIsStreaming(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetMediaIsStreaming(mediaRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetMediaLocation(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetMediaLocation(mediaRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetMediaCachePath(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetMediaCachePath(mediaRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeleteMediaRef(global::System.IntPtr mediaRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Linux.DeleteMediaRef(mediaRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    /*
     * Events
    */
    
    public static global::System.IntPtr GetEventRefString(string soundBankName)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetEventRefString(soundBankName);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetEventName(global::System.IntPtr eventRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetEventName(eventRefPtr));

        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetEventPath(global::System.IntPtr eventRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetEventPath(eventRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static System.IntPtr GetEventGuid(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetEventGuid(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetEventShortId(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetEventShortId(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static float GetEventMaxAttenuation(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetEventMaxAttenuation(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static float GetEventMinDuration(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetEventMinDuration(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static float GetEventMaxDuration(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetEventMaxDuration(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static global::System.IntPtr GetEventMedia(global::System.IntPtr soundBankRefPtr, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetEventMedia(soundBankRefPtr, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }    
    public static uint GetEventMediasCount(global::System.IntPtr soundBankRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetEventMediasCount(soundBankRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeleteEventRef(global::System.IntPtr eventRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Linux.DeleteEventRef(eventRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    /*
     * Platform
    */
    
    public static global::System.IntPtr GetPlatformRef(string soundBankName)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetPlatformRef(soundBankName);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetPlatformName(global::System.IntPtr platformRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetPlatformName(platformRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static global::System.IntPtr GetPlatformGuid(global::System.IntPtr platformRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetPlatformGuid(platformRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeletePlatformRef(global::System.IntPtr platformRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Linux.DeletePlatformRef(platformRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    /**
     * Plugin
     */
    
    public static global::System.IntPtr GetAllPluginRef()
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetAllPluginRef();
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    public static uint GetPluginCount()
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetPluginCount();
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }

    public static global::System.IntPtr GetPluginRefIndex(global::System.IntPtr soundBankArrayRef, int index)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetPluginRefIndex(soundBankArrayRef, index);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static uint GetPluginId(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            return WwiseProjectDatabasePINVOKE_Linux.GetPluginId(pluginRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static string GetPluginName(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetPluginName(pluginRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    public static string GetPluginDLL(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetPluginDLL(pluginRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    public static string GetPluginStaticLib(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            return WwiseProjectDatabase.StringFromIntPtrString(WwiseProjectDatabasePINVOKE_Linux.GetPluginStaticLib(pluginRefPtr));
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    public static void DeletePluginRef(global::System.IntPtr pluginRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Linux.DeletePluginRef(pluginRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
    
    public static void DeletePluginArrayRef(global::System.IntPtr pluginArrayRefPtr)
    {
        try
        {
            WwiseProjectDatabasePINVOKE_Linux.DeletePluginArrayRef(pluginArrayRefPtr);
        }
        catch (System.Exception e)
        {
            LogProjectDatabaseDLLException(e);
            throw;
        }
    }
}
#endif