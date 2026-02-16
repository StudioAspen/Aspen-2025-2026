// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using UnityEngine;
namespace Febucci.TextAnimatorForUnity.Core
{
    class MonoDispatcher : MonoBehaviour
    {
        static MonoDispatcher instance;
        public static MonoDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("Text Animator MonoDispatcher").AddComponent<MonoDispatcher>();
                    DontDestroyOnLoad(instance.gameObject);
                    instance.hideFlags = HideFlags.HideAndDontSave;
                }

                return instance;
            }
        }
    }
}