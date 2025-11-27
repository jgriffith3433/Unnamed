using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	public class OneClickThomas : OneClickBase
	{
		/// <summary>
		/// Custom OneClick for Thomas character (David Grette Adventure Pack)
		/// Uses ARKit-standard blendshape names
		/// </summary>
		public static void Setup(GameObject gameObject)
		{
			////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//	SETUP Requirements:
			//		use NewExpression("expression name") to start a new viseme/emote expression.
			//		use AddShapeComponent to add blendshape configurations, passing:
			//			- string array of shape names to look for.
			//			  : string array can be a single element.
			//			  : string array can be a single regex search string.
			//			    note: useRegex option must be set true.
			//			- optional string name prefix for the component.
			//			- optional blend amount (default = 1.0f).
			//			- optional regex search option (default = false).

			Init();

			#region SALSA-Configuration
			NewConfiguration(OneClickConfiguration.ConfigType.Salsa);
			{
				////////////////////////////////////////////////////////
				// SMR regex searches (enable/disable/add as required).
				// Matches Thomas character head meshes
				AddSmrSearch("^.+(Thomas|Head).*$");

				////////////////////////////////////////////////////////
				// Adjust SALSA settings to taste...
				// - data analysis settings
				autoAdjustAnalysis = true;
				autoAdjustMicrophone = false;
				audioUpdateDelay = 0.089798f;

				// - advanced dynamics settings
				loCutoff = 0.01434442f;
				hiCutoff = 0.8047945f;
				useAdvDyn = true;
				advDynPrimaryBias = 0.5f;
				useAdvDynJitter = false;
				advDynJitterAmount = 0.25f;
				advDynJitterProb = 0.25f;
				advDynSecondaryMix = 0f;
				emphasizerTrigger = 0f;

				////////////////////////////////////////////////////////
				// Viseme setup using ARKit blendshapes...

				NewExpression("w");
				AddShapeComponent(new[] { "Head_blendShape.jawOpen" }, 0.08f, 0f, 0.08f, "jawOpen", 0.0009f, false);
				AddShapeComponent(new []{"Head_blendShape.mouthFunnel"}, 0.08f, 0f, 0.08f, "mouthFunnel", 0.5f, false);
				AddShapeComponent(new []{"Head_blendShape.mouthPucker"}, 0.08f, 0f, 0.08f, "mouthPucker", 0.4f, false);

				NewExpression("f");
				AddShapeComponent(new []{"Head_blendShape.mouthUpperUpLeft"}, 0.08f, 0f, 0.08f, "mouthUpperUpLeft", 0.3f, false);
				AddShapeComponent(new []{"Head_blendShape.mouthUpperUpRight"}, 0.08f, 0f, 0.08f, "mouthUpperUpRight", 0.3f, false);

				NewExpression("t");
				AddShapeComponent(new []{"Head_blendShape.jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.2f, false);
				AddShapeComponent(new []{"Head_blendShape.mouthClose"}, 0.08f, 0f, 0.08f, "mouthClose", 0.5f, false);

				NewExpression("th");
				AddShapeComponent(new []{"Head_blendShape.jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.15f, false);

				NewExpression("ow");
				AddShapeComponent(new []{"Head_blendShape.mouthFunnel"}, 0.08f, 0f, 0.08f, "mouthFunnel", 0.6f, false);
				AddShapeComponent(new []{"Head_blendShape.jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.3f, false);

				NewExpression("ee");
				AddShapeComponent(new []{"Head_blendShape.mouthSmileLeft"}, 0.08f, 0f, 0.08f, "mouthSmileLeft", 0.5f, false);
				AddShapeComponent(new []{"Head_blendShape.mouthSmileRight"}, 0.08f, 0f, 0.08f, "mouthSmileRight", 0.5f, false);
				AddShapeComponent(new []{"Head_blendShape.jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.1f, false);

				NewExpression("oo");
				AddShapeComponent(new []{"Head_blendShape.jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.4f, false);
				AddShapeComponent(new []{"Head_blendShape.mouthFunnel"}, 0.08f, 0f, 0.08f, "mouthFunnel", 0.3f, false);
			}
			#endregion // SALSA-configuration

			#region EmoteR-Configuration
			NewConfiguration(OneClickConfiguration.ConfigType.Emoter);
			{
				////////////////////////////////////////////////////////
				// SMR regex searches (enable/disable/add as required).
				AddSmrSearch("^.+(Thomas|Head).*$");

				useRandomEmotes = false;
				isChancePerEmote = true;
				numRandomEmotesPerCycle = 0;
				randomEmoteMinTimer = 1f;
				randomEmoteMaxTimer = 2f;
				randomChance = 0.5f;
				useRandomFrac = false;
				randomFracBias = 0.5f;
				useRandomHoldDuration = false;
				randomHoldDurationMin = 0.1f;
				randomHoldDurationMax = 0.5f;

				////////////////////////////////////////////////////////
				// Emote setup...

				NewExpression("smile");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new []{"Head_blendShape.mouthSmileLeft"}, 0.25f, 0.1f, 0.2f, "mouthSmileLeft", 0.6f, false);
				AddShapeComponent(new []{"Head_blendShape.mouthSmileRight"}, 0.25f, 0.1f, 0.2f, "mouthSmileRight", 0.6f, false);

				NewExpression("browsUp");
				AddEmoteFlags(false, true, false, 0.427f, true);
				AddShapeComponent(new []{"Head_blendShape.eyeSquintLeft"}, 0.25f, 0.1f, 0.2f, "eyeSquintLeft", 0.3f, false);
				AddShapeComponent(new []{"Head_blendShape.eyeSquintRight"}, 0.25f, 0.1f, 0.2f, "eyeSquintRight", 0.3f, false);

				NewExpression("frown");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new []{"Head_blendShape.mouthFrownLeft"}, 0.25f, 0.1f, 0.2f, "mouthFrownLeft", 0.5f, false);
				AddShapeComponent(new []{"Head_blendShape.mouthFrownRight"}, 0.25f, 0.1f, 0.2f, "mouthFrownRight", 0.5f, false);

				NewExpression("pucker");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new []{"Head_blendShape.mouthPucker"}, 0.25f, 0.1f, 0.2f, "mouthPucker", 0.6f, false);

				NewExpression("squint");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new []{"Head_blendShape.eyeSquintLeft"}, 0.25f, 0.1f, 0.2f, "eyeSquintLeft", 0.5f, false);
				AddShapeComponent(new []{"Head_blendShape.eyeSquintRight"}, 0.25f, 0.1f, 0.2f, "eyeSquintRight", 0.5f, false);

				NewExpression("wide");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new []{"Head_blendShape.eyeWideLeft"}, 0.25f, 0.1f, 0.2f, "eyeWideLeft", 0.5f, false);
				AddShapeComponent(new []{"Head_blendShape.eyeWideRight"}, 0.25f, 0.1f, 0.2f, "eyeWideRight", 0.5f, false);
			}
			#endregion // EmoteR-configuration

			DoOneClickiness(gameObject);

			if (selectedObject.GetComponent<SalsaAdvancedDynamicsSilenceAnalyzer>() == null)
				selectedObject.AddComponent<SalsaAdvancedDynamicsSilenceAnalyzer>();

			//Timing tweaks
			salsa.useTimingsOverride = true;
			salsa.globalDurON = 0.118f;
			salsa.globalDurOffBalance = -0.09f;
			salsa.globalNuanceBalance = -0.239f;

			emoter.NumRandomEmphasizersPerCycle = 4;

			var silenceAnalyzer = selectedObject.GetComponent<SalsaAdvancedDynamicsSilenceAnalyzer>();
			if (silenceAnalyzer)
			{
				silenceAnalyzer.silenceThreshold = 0.9f;
				silenceAnalyzer.timingStartPoint = 0.4f;
				silenceAnalyzer.timingEndVariance = 0.8f;
				silenceAnalyzer.silenceSampleWeight = 0.95f;
				silenceAnalyzer.bufferSize = 512;
			}
		}
	}
}
