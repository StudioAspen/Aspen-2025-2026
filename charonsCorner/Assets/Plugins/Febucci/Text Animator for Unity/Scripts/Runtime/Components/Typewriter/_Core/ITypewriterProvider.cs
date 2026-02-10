// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

namespace Febucci.TextAnimatorForUnity
{
    interface ITypewriterProvider
    {
        /// <summary>
        /// Sets the given text to the connected TextAnimator component.<br/>
        /// If enabled, it also starts showing letters dynamically. <br/>
        /// - Manual: <see href="https://www.febucci.com/text-animator-unity/docs/text-animator-players/">Text Animator Players</see>
        /// </summary>
        /// <param name="text"></param>
        /// <remarks>
        /// If the typewriter is enabled but its start mode <see cref="startTypewriterMode"/> doesn't include <see cref="StartTypewriterMode.OnShowText"/>, this method won't start showing letters. You'd have to manually call <see cref="StartShowingText"/> in order to start the typewriter, or include different "start modes" like <see cref="StartTypewriterMode.OnEnable"/> and let the script manage it automatically.
        /// </remarks>
        public void ShowText(string text);

        /// <summary>
        /// Skips the typewriter animation (if it's currently showing).<br/>
        /// In case the text is revealing, it will show all the letters immediately.<br/>
        /// In case the text is hiding, it will hide all the letters immediately.
        /// </summary>
        /// <remarks>
        /// If both revealing and hiding are occurring, hiding will prevail.
        /// </remarks>
        public void SkipTypewriter();


        /// <summary>
        /// True if the typewriter is currently showing letters
        /// </summary>
        public bool IsShowingText { get; }

        /// <summary>
        /// Starts showing letters dynamically
        /// </summary>
        /// <param name="restart"><code>false</code> if you want the typewriter to resume where it has left.
        /// <code>true</code> if the typewriter should restart from character 0</param>
        public void StartShowingText(bool restart = false);

        /// <summary>
        /// Stops showing letters dynamically, leaving the text as it is.
        /// </summary>
        public void StopShowingText();

        /// <summary>
        /// True if the typewriter is currently disappearing the text
        /// </summary>
        public bool IsHidingText { get; }


        /// <summary>
        /// Starts disappearing the text dynamically
        /// </summary>
        public void StartDisappearingText();

        /// <summary>
        /// Stops the typewriter's from disappearing the text dynamically, leaving the text at its current state
        /// </summary>
        public void StopDisappearingText();

        /// <summary>
        /// Makes the typewriter slower/faster, by setting its internal speed multiplier.
        /// </summary>
        /// <param name="speed"></param>
        /// <example>
        /// If the typewriter has to wait <c>1</c> second to show the next letter but you set the typewriter speed to <c>2</c>, the typewriter will wait <c>0.5</c> seconds.
        /// </example>
        /// <remarks>
        /// The minimum value is 0.001
        /// </remarks>
        public void SetTypewriterSpeed(float speed);


        /// <summary>
        /// Triggers all messages/events that have not been triggered, but that are in the visible range of the text.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TriggerRemainingEvents"/>
        /// </remarks>
        public void TriggerVisibleEvents();


        /// <summary>
        /// Triggers all messages/events that have not yet been triggered, even if they're not shown in the yet.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TriggerVisibleEvents"/>
        /// </remarks>
        public void TriggerRemainingEvents();
    }
}