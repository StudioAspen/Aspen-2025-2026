// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorForUnity;
using UnityEngine;
using UnityEngine.Assertions;

namespace Febucci.UI.Examples
{
    /// <summary>
    /// Extra example class for the TextAnimator plugin, used to add sounds to the TextAnimatorPlayer.
    /// </summary>
    [AddComponentMenu("Febucci/TextAnimator/SoundWriter")]
    [RequireComponent(typeof(TypewriterComponent))]
    public class TAnimSoundWriter : MonoBehaviour
    {

        [Header("References")]
        public AudioSource source;

        [Header("Management")]
        [Tooltip("How much time has to pass before playing the next sound"), SerializeField, Attributes.MinValue(0)]
        float minSoundDelay = .07f;

        [Tooltip("True if you want the new sound to cut the previous one\nFalse if each sound will continue until its end"), SerializeField]
        bool interruptPreviousSound = true;

        [Header("Audio Clips")]
        [Tooltip("True if sounds will be picked random from the array\nFalse if they'll be chosen in order"), SerializeField]
        bool randomSequence = false;
        [SerializeField] AudioClip[] sounds = new AudioClip[0];

        float latestTimePlayed = -1;
        int clipIndex;

        private void Awake()
        {
            Assert.IsNotNull(source, "TAnimSoundWriter: Typewriter Audio Source reference is null");
            Assert.IsNotNull(sounds, "TAnimSoundWriter: Sounds clips array is null in the");
            Assert.IsTrue(sounds.Length > 0, "TAnimSoundWriter: Sounds clips array is empty");
            Assert.IsNotNull(GetComponent<TypewriterComponent>(), "TAnimSoundWriter: Component TAnimPlayerBase is not present");


            //Prevents subscribing the event if the component has not been set correctly
            if (source == null || sounds.Length <= 0)
                return;

            //Prevents common setup errors
            source.playOnAwake = false;
            source.loop = false;

            GetComponent<TypewriterComponent>()?.onCharacterVisible.AddListener(OnCharacter);

            clipIndex = randomSequence ? Random.Range(0, sounds.Length) : 0;
        }

        void OnCharacter(CharacterData character)
        {
            if (Time.time - latestTimePlayed <= minSoundDelay)
                return; //Early return if not enough time passed yet

            source.clip = sounds[clipIndex];

            //Plays audio
            if (interruptPreviousSound)
                source.Play();
            else
                source.PlayOneShot(source.clip);

            //Chooses next clip to play
            if (randomSequence)
            {
                clipIndex = Random.Range(0, sounds.Length);
            }
            else
            {
                clipIndex++;
                if (clipIndex >= sounds.Length)
                    clipIndex = 0;
            }

            latestTimePlayed = Time.time;

        }
    }
}