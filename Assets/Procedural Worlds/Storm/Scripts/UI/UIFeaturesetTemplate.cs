using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using System.Collections.Generic;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Featureset Template")]
    public class UIFeaturesetTemplate : MonoBehaviour
    {
        public Toggle DisableButton;
        public Text NameText;
        public TMP_Text NameTMPText;
        public Transform FeatureListArea;
        public UIFeatureTemplate FeatureTemplate;

        private StormWorld World;
        private StormFeatureset Featureset;

        public void SetFeatureset(StormFeatureset featureset, StormWorld world, UIStormPerformanceDisplay.FeatureDisplaySet[] featureDisplays)
        {
            World = world;
            Featureset = featureset;
            if (Featureset == null)
            {
                if (NameText != null)
                    NameText.text = "NULL";
                if (NameTMPText != null)
                    NameTMPText.text = "NULL";

                if (DisableButton != null)
                {
                    DisableButton.interactable = false;
                }
                if (FeatureListArea == null || FeatureTemplate == null)
                    return;

                List<GameObject> toDestroy = new List<GameObject>(FeatureListArea.childCount - 1);
                for (int c = 0; c < FeatureListArea.childCount; ++c)
                {
                    Transform child = FeatureListArea.GetChild(c);
                    if (child == null)
                        continue;
                    if (child.GetComponent<UIFeatureTemplate>() == FeatureTemplate)
                    {
                        child.gameObject.SetActive(false);
                        continue;
                    }
                    toDestroy.Add(child.gameObject);
                }
                toDestroy.ForEach(StormUtils.Destroy);
            }
            else
            {
                if (NameText != null)
                    NameText.text = Featureset.name;
                if (NameTMPText != null)
                    NameTMPText.text = Featureset.name;

                if (FeatureListArea == null || FeatureTemplate == null)
                    return;
                List<GameObject> toDestroy = new List<GameObject>(FeatureListArea.childCount - 1);
                for (int c = 0; c < FeatureListArea.childCount; ++c)
                {
                    Transform child = FeatureListArea.GetChild(c);
                    if (child == null)
                        continue;
                    if (child.GetComponent<UIFeatureTemplate>() == FeatureTemplate)
                    {
                        child.gameObject.SetActive(false);
                        continue;
                    }
                    toDestroy.Add(child.gameObject);
                }
                toDestroy.ForEach(StormUtils.Destroy);
                bool allDisabled = true;
                for (int f = 0; f < Featureset.Features.Length; ++f)
                {
                    if (Featureset.Features[f].Feature == null)
                        continue;
                    UIFeatureTemplate item = Instantiate(FeatureTemplate, FeatureListArea);
                    item.gameObject.SetActive(true);
                    item.SetFeature(Featureset, Featureset.Features[f].Feature, World);
                    item.OnDisableChanged.AddListener(OnChildDisabled);
                    if (!World.SystemSettings.IsFeatureDisabled(Featureset, Featureset.Features[f].Feature))
                        allDisabled = false;
                    int idx = StormDataCache.AllEnabledFeatures.IndexOf(featureset.Features[f].Feature);
                    if (idx >= 0 && idx < featureDisplays.Length)
                        featureDisplays[idx] = item.PerformanceDisplay; 
                }
                if (DisableButton != null)
                {
                    DisableButton.isOn = allDisabled;
                    DisableButton.onValueChanged.AddListener(OnDisableToggle);
                }
            }
        }
        private void OnChildDisabled()
        {
            bool allDisabled = true;
            for (int f = 0; f < Featureset.Features.Length; ++f)
            {
                if (Featureset.Features[f].Feature == null)
                    continue;
                if (!World.SystemSettings.IsFeatureDisabled(Featureset, Featureset.Features[f].Feature))
                    allDisabled = false;
            }
            if (DisableButton != null)
            {
                DisableButton.isOn = allDisabled;
                DisableButton.onValueChanged.AddListener(OnDisableToggle);
            }
        }
        public void OnDisableToggle(bool disabled)
        {
            bool needsUpdate = false;
            for (int f = 0; f < Featureset.Features.Length; ++f)
            {
                if (World.Debug_SoloFeatureIdx == StormDataCache.GetFeatureIndex(Featureset.Features[f].Feature))
                    World.Debug_SoloFeatureIdx = -1;
                if (World.SystemSettings.SetFeatureDisabled(Featureset, Featureset.Features[f].Feature, disabled))
                    needsUpdate = true;
            }
            if (needsUpdate)
                World.ForceFullRefresh();
        }
    }
}