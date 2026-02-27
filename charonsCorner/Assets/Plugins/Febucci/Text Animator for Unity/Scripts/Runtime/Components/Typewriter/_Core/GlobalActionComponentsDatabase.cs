// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System.Collections.Generic;
using Febucci.TextAnimatorCore.Data;
using Febucci.TextAnimatorCore.Typing;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity
{
    class GlobalActionComponentsDatabase : MonoBehaviour, IDatabaseProvider<ITypewriterAction>
    {
        static GlobalActionComponentsDatabase instance;
        public static GlobalActionComponentsDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("Text Animator - Global Action Components Database").AddComponent<GlobalActionComponentsDatabase>();
                    DontDestroyOnLoad(instance.gameObject);
                    instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    instance.Database = new Dictionary<string, ITypewriterAction>();
                    Logger.Debug($"Created {nameof(GlobalActionComponentsDatabase)} instance", instance.gameObject);
                }

                return instance;
            }
        }

        public void Register(ITypewriterAction action)
        {
            // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd
            if (!Database.ContainsKey(action.TagID))
            {
                Database.Add(action.TagID, action);
                Logger.Debug($"Registering action: {action.TagID}");
            }
            else
            {
                Logger.Debug($"SKIPPED registering action: {action.TagID}. Already present.");
            }
        }

        public void Unregister(ITypewriterAction action)
        {
            if (Database.ContainsKey(action.TagID) && Database.ContainsValue(action))
            {
                Database.Remove(action.TagID);
                Logger.Debug($"Removed action: {action.TagID}");
            }
            else
            {
                Logger.Debug($"Skipped removing action: {action.TagID}");
            }
        }

        public Dictionary<string, ITypewriterAction> Database { get; private set; }

        void OnDestroy()
        {
            instance = null;
        }
    }
}