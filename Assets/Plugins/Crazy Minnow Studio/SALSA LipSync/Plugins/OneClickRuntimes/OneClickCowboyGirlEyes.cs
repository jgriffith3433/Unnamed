using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	public class OneClickCowboyGirlEyes : MonoBehaviour
	{
		public static void Setup(GameObject go)
		{
			string head = "^head$";
			string[] body = new string[] {"Head"};
			string[] blinkL = new string[] {"eyeBlinkLeft"};
			string[] blinkR = new string[] {"eyeBlinkRight"};

			if (go)
			{
				Eyes eyes = go.GetComponent<Eyes>();
				if (eyes == null)
				{
					eyes = go.AddComponent<Eyes>();
				}
				else
				{
					DestroyImmediate(eyes);
					eyes = go.AddComponent<Eyes>();
				}

				// System Properties
				eyes.characterRoot = go.transform;
				
				// Disable head tracking to avoid conflicts with animations
				eyes.headEnabled = false;

				// Find SkinnedMeshRenderer with blendshapes (Head child object)
				SkinnedMeshRenderer smr = null;
				var allSMRs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (var renderer in allSMRs)
				{
					if (renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0)
					{
						// Check if it's the Head mesh
						if (renderer.gameObject.name.Contains("Head"))
						{
							smr = renderer;
							Debug.Log($"OneClickCowboyGirl: Found Head SkinnedMeshRenderer on '{renderer.gameObject.name}' with {renderer.sharedMesh.blendShapeCount} blendshapes");
							break;
						}
					}
				}

				if (smr == null)
				{
					Debug.LogError("OneClickCowboyGirl: Could not find Head SkinnedMeshRenderer with blendshapes.");
					return;
				}

				// Don't build head template - head tracking disabled to avoid animation conflicts
				// (Head tracking can interfere with Timeline animations and character positioning)

				// Eyelids - BlendShapes
				eyes.BuildEyelidTemplate(Eyes.EyelidTemplates.BlendShapes, Eyes.EyelidSelection.Upper);
				float blinkMax = 1f;
				
				// Left eyelid
				eyes.blinklids[0].expData.controllerVars[0].smr = smr;
				eyes.blinklids[0].expData.controllerVars[0].blendIndex = Eyes.FindBlendIndex(smr, blinkL);
				eyes.blinklids[0].expData.controllerVars[0].maxShape = blinkMax;
				eyes.blinklids[0].expData.name = "eyelidL";
				
				// Right eyelid
				eyes.blinklids[1].expData.controllerVars[0].smr = smr;
				eyes.blinklids[1].expData.controllerVars[0].blendIndex = Eyes.FindBlendIndex(smr, blinkR);
				eyes.blinklids[1].expData.controllerVars[0].maxShape = blinkMax;
				eyes.blinklids[1].expData.name = "eyelidR";

				// Track lids - copy blink to track
				eyes.CopyBlinkToTrack();
				if (eyes.tracklids.Count > 1)
				{
					eyes.tracklids[0].referenceIdx = 0; // left eye
					eyes.tracklids[1].referenceIdx = 1; // right eye
				}

				// Initialize the Eyes module
				eyes.Initialize();
			}
		}
	}
}
