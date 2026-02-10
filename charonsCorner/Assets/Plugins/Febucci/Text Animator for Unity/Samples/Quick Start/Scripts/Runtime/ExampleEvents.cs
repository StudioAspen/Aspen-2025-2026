// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;
using System.Collections;
using Febucci.Parsing;
using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorForUnity;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#endif

namespace Febucci.UI.Examples
{
    [AddComponentMenu(""), DisallowMultipleComponent]
    class ExampleEvents : MonoBehaviour
    {
        // ---- OTHER PART OF THE SCRIPT ----
        // This makes the script run faking a dialogue system
        [SerializeField] TypewriterComponent typewriter;
        [SerializeField, TextArea(1, 5)] string[] dialoguesLines;
        [SerializeField] Sprite[] faces;
        [SerializeField] SpriteRenderer faceRenderer;
        [SerializeField] GameObject continueText;
        [SerializeField] Transform[] crates;
        Vector3[] cratesInitialScale;

#if ENABLE_INPUT_SYSTEM
        bool inputSystemPassed;
        IDisposable eventListener;
#endif


        // ---- PART OF THE SCRIPT THAT YOU'RE PROBABLY INTERESTED IT ----

        void Start()
        {
            //Subscribe to the event
            typewriter.onMessage.AddListener(OnMessage);

#if ENABLE_INPUT_SYSTEM
            inputSystemPassed = false;
            eventListener = InputSystem.onAnyButtonPress.CallOnce(ctrl => inputSystemPassed = true);
#endif

            dialogueIndex = 0;
            CurrentLineShown = false;
            typewriter.ShowText(dialoguesLines[dialogueIndex]);
        }

        void OnDestroy()
        {
            if(typewriter) typewriter.onMessage.RemoveListener(OnMessage);
#if ENABLE_INPUT_SYSTEM
            eventListener?.Dispose();
#endif
        }

        bool TryGetInt(string parameter, out int result)
        {

            if (FormatUtils.TryGetFloat(parameter, 0, out float resultFloat))
            {
                result = (int)resultFloat;
                return true;
            }

            result = -1;
            return false;
        }
        void OnMessage(EventMarker eventData)
        {
            switch (eventData.name)
            {
                case "face":
                    if (eventData.parameters.Length <= 0)
                    {
                        Debug.LogWarning($"You need to specify a sprite index! Dialogue: {dialogueIndex}");
                        return;
                    }

                    if (TryGetInt(eventData.parameters[0], out int spriteIndex))
                    {
                        if (spriteIndex >= 0 && spriteIndex < faces.Length)
                        {
                            faceRenderer.sprite = faces[spriteIndex];
                        }
                        else
                        {
                            Debug.Log($"Sprite index was out of range. Dialogue: {dialogueIndex}");
                        }
                    }
                    break;

                case "crate":
                    if (eventData.parameters.Length <= 0)
                    {
                        Debug.LogWarning($"You need to specify a crate index! Dialogue: {dialogueIndex}");
                        return;
                    }

                    if (TryGetInt(eventData.parameters[0], out int crateIndex))
                    {
                        if (crateIndex >= 0 && crateIndex < crates.Length)
                        {
                            StartCoroutine(AnimateCrate(crateIndex));
                        }
                        else
                        {
                            Debug.Log($"Sprite index was out of range. Dialogue: {dialogueIndex}");
                        }
                    }
                    break;
            }
        }

        int dialogueIndex = 0;
        int dialogueLength;
        bool currentLineShown;

        bool CurrentLineShown
        {
            get => currentLineShown;
            set
            {
                currentLineShown = value;
                continueText.SetActive(value);
            }
        }

        void Awake()
        {
            cratesInitialScale = new Vector3[crates.Length];
            for (int i = 0; i < crates.Length; i++)
            {
                cratesInitialScale[i] = crates[i].localScale;
            }

            dialogueLength = dialoguesLines.Length;
            typewriter.onTextShowed.AddListener(() => CurrentLineShown = true);
        }

        void ContinueSequence()
        {
            CurrentLineShown = false;
            dialogueIndex++;
            if(dialogueIndex<dialogueLength)
            {
                typewriter.ShowText(dialoguesLines[dialogueIndex]);
            }
            else
            {
                typewriter.StartDisappearingText();
            }
        }

        void Update()
        {
            if (!CurrentLineShown) return;

            bool inputDetected = false;

#if ENABLE_LEGACY_INPUT_MANAGER
            inputDetected = Input.anyKeyDown;
#endif

#if ENABLE_INPUT_SYSTEM
            if (inputSystemPassed)
            {
                inputDetected = true;
                inputSystemPassed = false;
                eventListener = InputSystem.onAnyButtonPress.CallOnce(ctrl => inputSystemPassed = true);
            }
#endif

            if (inputDetected)
            {
                ContinueSequence();
            }
        }

        IEnumerator AnimateCrate(int crateIndex)
        {
            Transform crate = crates[crateIndex];
            Vector3 initialScale = cratesInitialScale[crateIndex];
            Vector3 targetScale = new Vector3(initialScale.x * 1.2f, initialScale.y * .6f, initialScale.z);
            float t = 0;
            const float duration = .4f;

            while (t<=duration)
            {
                t += Time.unscaledDeltaTime;
                float pct = t / duration;
                if (pct < .5f) pct = pct / .5f;
                else pct = 1 - (pct - .5f) / .5f;

                crate.localScale = Vector3.LerpUnclamped(initialScale, targetScale, pct);
                yield return null;
            }

            crate.localScale = initialScale;
        }
    }

}