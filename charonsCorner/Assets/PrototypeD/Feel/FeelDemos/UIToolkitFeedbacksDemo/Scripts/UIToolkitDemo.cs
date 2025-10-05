using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace MoreMountains.FeedbacksForThirdParty
{
	[Serializable]
	public class UIToolkitDemoAction
	{
		public string ButtonName;
		public MMF_Player TargetPlayer;
	}

	public class UIToolkitDemo : MonoBehaviour
	{
		public Texture2D FaceTexture;
		public List<UIToolkitDemoAction> Actions;

		private Button _button;
	
	
		private void OnEnable()
		{
			VisualElement root = GetComponent<UIDocument>().rootVisualElement;

			VisualElement face = root.Q<VisualElement>("DemoFace");
			face.style.backgroundImage = FaceTexture;
		
			foreach (UIToolkitDemoAction action in Actions)
			{
				_button = root.Q<Button>(action.ButtonName);
				_button.text = _button.text.ToUpper();
				_button.RegisterCallback<ClickEvent>(ev => PlayFeedback(action.TargetPlayer));
			}
		}

		private void PlayFeedback(MMF_Player player)
		{
			player.PlayFeedbacks();
		}
	}
}


