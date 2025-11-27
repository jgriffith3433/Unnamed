using UnityEditor;
using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	/// <summary>
	/// Custom OneClick Editor for SK_Cowboy_Girl_2 character (David Grette Cowboy Pack)
	/// Uses ARKit-standard blendshape names
	/// </summary>
	public class OneClickCowboyGirlEditor : Editor
	{
		[MenuItem("GameObject/Crazy Minnow Studio/SALSA LipSync/One-Clicks/Cowboy Girl (ARKit)")]
		public static void OneClickSetup_CowboyGirl()
		{
			GameObject go = Selection.activeGameObject;
			if (go == null)
			{
				Debug.LogWarning(
					"NO OBJECT SELECTED: You must select an object in the scene to apply the OneClick to.");
				return;
			}

			ApplyOneClick(go);
		}

		private static void ApplyOneClick(GameObject go)
		{
			OneClickCowboyGirl.Setup(go);
			OneClickCowboyGirlEyes.Setup(go);

			// add QueueProcessor
			OneClickBase.AddQueueProcessor(go);
			
			// config AudioSource
			var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(OneClickBase.RESOURCE_CLIP);
			OneClickBase.ConfigureSalsaAudioSource(go, clip, true);
		}
	}
}
