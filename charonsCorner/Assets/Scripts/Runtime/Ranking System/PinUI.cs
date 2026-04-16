using UnityEngine;
using System;

namespace CharonsCorner.Runtime
{
    public class PinUI : MonoBehaviour
    {
        public Action OnAllowSubtractTime;
        
        public void AllowSubtractTime()
        {
                OnAllowSubtractTime.Invoke();
        }
    }
}
