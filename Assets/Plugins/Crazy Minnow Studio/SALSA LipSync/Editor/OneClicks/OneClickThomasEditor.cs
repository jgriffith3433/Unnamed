using UnityEditor;
using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	/// <summary>
	/// Custom OneClick Editor for Thomas character (David Grette Adventure Pack)
	/// Uses ARKit-standard blendshape names
	/// </summary>
	public class OneClickThomasEditor : Editor
	{
		[MenuItem("GameObject/Crazy Minnow Studio/SALSA LipSync/One-Clicks/Thomas (ARKit)")]
		public static void OneClickSetup_Thomas()
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
			OneClickThomas.Setup(go);
			OneClickThomasEyes.Setup(go);

			// add QueueProcessor
			OneClickBase.AddQueueProcessor(go);
			
			// config AudioSource
			var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(OneClickBase.RESOURCE_CLIP);
			OneClickBase.ConfigureSalsaAudioSource(go, clip, true);
		}
	}
}
