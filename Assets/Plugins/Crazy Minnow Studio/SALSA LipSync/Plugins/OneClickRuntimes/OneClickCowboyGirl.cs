using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	public class OneClickCowboyGirl : OneClickBase
	{
		/// <summary>
		/// Custom OneClick for SK_Cowboy_Girl_2 character (David Grette Cowboy Pack)
		/// Uses ARKit-standard blendshape names
		/// </summary>
		public static void Setup(GameObject gameObject)
		{
			Init();

			#region SALSA-configuration
			NewConfiguration(OneClickConfiguration.ConfigType.Salsa);
			{
				////////////////////////////////////////////////////////
				// SMR search string.
				//	Search for mesh named "Head" (exact match or contains)
				AddSmrSearch("Head");

				////////////////////////////////////////////////////////
				// SALSA settings
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
				// Note: Try both with and without "Head_blendShape." prefix

				NewExpression("w");
				AddShapeComponent(new[] { "jawOpen" }, 0.08f, 0f, 0.08f, "jawOpen", 0.0009f, false);
				AddShapeComponent(new []{"mouthFunnel"}, 0.08f, 0f, 0.08f, "mouthFunnel", 0.5f, false);
				AddShapeComponent(new []{"mouthPucker"}, 0.08f, 0f, 0.08f, "mouthPucker", 0.4f, false);

				NewExpression("f");
				AddShapeComponent(new []{"mouthUpperUpLeft"}, 0.08f, 0f, 0.08f, "mouthUpperUpLeft", 0.3f, false);
				AddShapeComponent(new []{"mouthUpperUpRight"}, 0.08f, 0f, 0.08f, "mouthUpperUpRight", 0.3f, false);

				NewExpression("t");
				AddShapeComponent(new []{"jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.2f, false);
				AddShapeComponent(new []{"mouthClose"}, 0.08f, 0f, 0.08f, "mouthClose", 0.5f, false);

				NewExpression("th");
				AddShapeComponent(new []{"jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.15f, false);

				NewExpression("ow");
				AddShapeComponent(new []{"mouthFunnel"}, 0.08f, 0f, 0.08f, "mouthFunnel", 0.6f, false);
				AddShapeComponent(new []{"jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.3f, false);

				NewExpression("ee");
				AddShapeComponent(new []{"mouthSmileLeft"}, 0.08f, 0f, 0.08f, "mouthSmileLeft", 0.5f, false);
				AddShapeComponent(new []{"mouthSmileRight"}, 0.08f, 0f, 0.08f, "mouthSmileRight", 0.5f, false);
				AddShapeComponent(new []{"jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.1f, false);

				NewExpression("oo");
				AddShapeComponent(new []{"jawOpen"}, 0.08f, 0f, 0.08f, "jawOpen", 0.4f, false);
				AddShapeComponent(new []{"mouthFunnel"}, 0.08f, 0f, 0.08f, "mouthFunnel", 0.3f, false);
			}
			#endregion // SALSA-configuration

			#region EmoteR-Configuration
			NewConfiguration(OneClickConfiguration.ConfigType.Emoter);
			{
				////////////////////////////////////////////////////////
				// EMOTER SETTINGS
				useRandomEmotes = true;
				isChancePerEmote = true;
				numRandomEmotesPerCycle = 4;
				useRandomFrac = false;
				randomFracBias = 0.5f;
				useRandomHoldDuration = false;
				randomHoldDurationMax = 0.5f;
				randomHoldDurationMin = 0.1f;
				////////////////////////////////////////////////////////
				// EMOTE SETUP
				//	 NOTE: For Emoter, regular expressions are assumed, but remember
				//			  to escape parens and other regex characters.

				NewExpression("smile");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new []{"mouthSmileLeft"}, 0.25f, 0.1f, 0.2f, "mouthSmileLeft", 0.6f, false);
				AddShapeComponent(new []{"mouthSmileRight"}, 0.25f, 0.1f, 0.2f, "mouthSmileRight", 0.6f, false);

				NewExpression("browsUp");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new []{"eyeSquintLeft"}, 0.25f, 0.1f, 0.2f, "eyeSquintLeft", 0.5f, false);
				AddShapeComponent(new []{"eyeSquintRight"}, 0.25f, 0.1f, 0.2f, "eyeSquintRight", 0.5f, false);

				NewExpression("wide");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new []{"eyeWideLeft"}, 0.25f, 0.1f, 0.2f, "eyeWideLeft", 0.5f, false);
				AddShapeComponent(new []{"eyeWideRight"}, 0.25f, 0.1f, 0.2f, "eyeWideRight", 0.5f, false);
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
			}
		}
	}
}
