// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerState
    public abstract partial class AnimancerState
    {
        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        private static bool _SkipNextExpectFade;

        private bool _ExpectFade;
#endif

        /************************************************************************************************************************/

        /// <summary>[Internal] Sets a flag for <see cref="OptionalWarning.ExpectFade"/>.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void SetExpectFade(AnimancerState state, float fadeDuration)
        {
#if UNITY_ASSERTIONS
            state._ExpectFade = fadeDuration > 0;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] Sets the next <see cref="AssertNotExpectingFade"/> call to be skipped.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        internal static void SkipNextExpectFade()
        {
#if UNITY_ASSERTIONS
            _SkipNextExpectFade = true;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] Call when playing a `state` without a fade to check <see cref="OptionalWarning.ExpectFade"/>.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        internal static void AssertNotExpectingFade(AnimancerState state)
        {
#if UNITY_ASSERTIONS
            if (_SkipNextExpectFade)
            {
                _SkipNextExpectFade = false;
                return;
            }

            if (state._ExpectFade)
            {
                state._ExpectFade = false;// Don't log again for the same state.
                OptionalWarning.ExpectFade.Log(
                    "A state was created by a transition with a non-zero Fade Duration" +
                    " but is now being played without a fade, which may be unintentional." +
                    " In most cases, the transition should be played so that it can properly" +
                    " apply its settings, unlike if the state is played directly.",
                    state.Graph?.Component);
            }
#endif
        }

        /************************************************************************************************************************/
    }
}

