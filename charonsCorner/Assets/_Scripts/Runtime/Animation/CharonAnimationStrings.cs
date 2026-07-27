using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "CharonAnimationStrings", menuName = "CharonsCorner/Animation/Charon Animation Strings")]
    public class CharonAnimationStrings : ScriptableObject
    {
        public List<string> AnimationEvents = new List<string>();
    }
}
