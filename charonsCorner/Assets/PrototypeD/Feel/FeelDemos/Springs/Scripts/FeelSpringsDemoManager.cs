using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feel
{
	[AddComponentMenu("")]
	public class FeelSpringsDemoManager : MonoBehaviour
	{
		[Header("Bindings")]
		public List<GameObject> DemoObjects;
		[MMReadOnly] public int CurrentIndex = 0;

		protected virtual void Start()
		{
			EnableCurrentDemo();
		}

		public virtual void NextDemo()
		{
			CurrentIndex++;
			if (CurrentIndex >= DemoObjects.Count)
			{
				CurrentIndex = 0;
			}
			EnableCurrentDemo();
		}

		public virtual void PreviousDemo()
		{
			CurrentIndex--;
			if (CurrentIndex < 0)
			{
				CurrentIndex = DemoObjects.Count - 1;
			}
			EnableCurrentDemo();
		}

		protected virtual void EnableCurrentDemo()
		{
			foreach (GameObject demoObject in DemoObjects)
			{
				demoObject.gameObject.SetActive(false);
			}
			DemoObjects[CurrentIndex].SetActive(true);
		}
	}
}
