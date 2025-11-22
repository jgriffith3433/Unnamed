using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Feature Display")]
    public class UIFeatureDisplay : MonoBehaviour
    {
        public Transform FeaturesetListArea;
        public UIFeaturesetTemplate FeaturesetTemplate;
        public UIStormPerformanceDisplay PerformanceDisplay;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (FeaturesetListArea == null || FeaturesetTemplate == null)
                return;

            List<GameObject> toDestroy = new List<GameObject>(FeaturesetListArea.childCount - 1);
            for (int c = 0; c < FeaturesetListArea.childCount; ++c)
            {
                Transform child = FeaturesetListArea.GetChild(c);
                if (child == null)
                    continue;
                if (child.GetComponent<UIFeaturesetTemplate>() == FeaturesetTemplate)
                {
                    child.gameObject.SetActive(false);
                    continue;
                }
                toDestroy.Add(child.gameObject);
            }
            toDestroy.ForEach(StormUtils.Destroy);

            StormWorld world = StormWorld.Instance;
            if (world == null)
                return;
            List<StormFeatureset> featuresets = StormDataCache.AllEnabledFeatureSets;
            UIStormPerformanceDisplay.FeatureDisplaySet[] featureDisplays = new UIStormPerformanceDisplay.FeatureDisplaySet[StormDataCache.AllEnabledFeatures.Count];
            for(int f = 0; f < featuresets.Count; ++f)
            {
                if (featuresets[f] == null)
                    continue;
                UIFeaturesetTemplate item = Instantiate(FeaturesetTemplate, FeaturesetListArea);
                item.gameObject.SetActive(true);
                item.SetFeatureset(featuresets[f], world, featureDisplays);
            }
            if (PerformanceDisplay != null)
                PerformanceDisplay.FeatureDisplays = featureDisplays;
        }
    }
}