using UnityEngine;
#if MM_UI
using UnityEngine.UI;
#endif
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace MoreMountains.Tools
{
	public class MMPlotterAxis : MonoBehaviour
	{
		#if MM_UI
		public Text Label;
		public Text TimeLabel;
		#endif
		public Transform PlotterCurvePoint;

		public Transform PositionPoint;
		public Transform PositionPointVertical;
		public Transform RotationPoint;
		public Transform ScalePoint;
        
		public virtual void SetLabel(string newLabel)
		{
			#if MM_UI
			Label.text = newLabel;
			#endif
		}
	}
}