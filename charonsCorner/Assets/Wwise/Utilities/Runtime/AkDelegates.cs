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
using System;
using UnityEngine;

public static class AkDelegates
{
    /// <summary>
    /// Invokes a parameterless Action, safely handling null/destroyed UnityEngine.Object targets.
    /// Automatically unsubscribes destroyed Unity objects.
    /// </summary>S
    /// <param name="action">The Action to invoke.</param>
    public static void InvokeUnitySafe(this System.Action action)
    {
        if (action == null) return;

        Delegate[] subscribers = action.GetInvocationList();

        foreach (Delegate del in subscribers)
        {
            // Check if the delegate's target is a UnityEngine.Object
            if (del.Target is UnityEngine.Object unityTarget)
            {
                // If the Unity object is null/destroyed, unsubscribe and skip
                if (unityTarget == null)
                {
                    Debug.Log($"WwiseUnity: Removing stale delegate from Action. Method: {del.Method.Name}, Target ID: {unityTarget.GetInstanceID()}");
                    action -= (Action)del;
                    continue;
                }
            }

            try
            {
                ((Action)del).Invoke();
            }
            catch (MissingReferenceException missingReferenceException)
            {
                Debug.LogError($"WwiseUnity: Missing Reference Exception caught during safe Invoke. Method: {del.Method.Name}. Error: {missingReferenceException.Message}");
                if (del.Target is UnityEngine.Object unityTargetOnException && unityTargetOnException == null)
                {
                    action -= (Action)del;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"WwiseUnity: Unexpected exception during safe Invoke. Method: {del.Method.Name}. Error: {ex.Message}");
            }
        }
    }
}