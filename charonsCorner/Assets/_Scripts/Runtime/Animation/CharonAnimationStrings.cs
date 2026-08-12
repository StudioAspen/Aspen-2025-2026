using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public abstract class AnimationStringsSO : ScriptableObject
    {
        public List<string> AnimationEvents = new List<string>();
    }

    [CreateAssetMenu(fileName = "CharonAnimationStrings", menuName = "CharonsCorner/Animation/Charon Animation Strings")]
    public class CharonAnimationStrings : AnimationStringsSO
    {
    }
}
