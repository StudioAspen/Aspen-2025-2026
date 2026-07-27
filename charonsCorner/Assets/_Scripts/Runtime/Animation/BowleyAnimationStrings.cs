using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "BowleyAnimationStrings", menuName = "CharonsCorner/Animation/Bowley Animation Strings")]
    public class BowleyAnimationStrings : ScriptableObject
    {
        public List<string> AnimationEvents = new List<string>();
    }
}
