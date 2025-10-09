// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// <see cref="ToggledSpeedSlider"/> for <see cref="AnimancerGraph"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerGraphSpeedSlider
    public class AnimancerGraphSpeedSlider : ToggledSpeedSlider
    {
        /************************************************************************************************************************/

        /// <summary>Singleton.</summary>
        public static readonly AnimancerGraphSpeedSlider
            Instance = new();

        /// <summary>The target graph.</summary>
        public AnimancerGraph Graph { get; set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerGraphSpeedSlider"/>.</summary>
        public AnimancerGraphSpeedSlider()
            : base(nameof(AnimancerGraphSpeedSlider) + ".Show")
        {
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnSetSpeed(float speed)
        {
            if (Graph != null)
                Graph.Speed = speed;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool DoToggleGUI(Rect area, GUIStyle style)
        {
            if (Graph != null)
                Speed = Graph.Speed;

            return base.DoToggleGUI(area, style);
        }

        /************************************************************************************************************************/
    }
}

#endif

