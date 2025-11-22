using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using UnityEngine.Events;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Feature Template")]
    public class UIFeatureTemplate : MonoBehaviour
    {
        public Toggle SoloButton;
        public Toggle DisableButton;
        public Text NameText;
        public TMP_Text NameTMPText;

        private StormWorld World;
        private StormFeatureset Featureset;
        private StormFeatureSettings Feature;

        [Space]
        public UIStormPerformanceDisplay.FeatureDisplaySet PerformanceDisplay;

        [Space]
        public UnityEvent OnDisableChanged;

        public void SetFeature(StormFeatureset featureset, StormFeatureSettings feature, StormWorld world)
        {
            PerformanceDisplay.Feature = feature;
            World = world;
            Featureset = featureset;
            Feature = feature;
            if (Feature == null)
            {
                if (NameText != null)
                    NameText.text = "NULL";
                if (NameTMPText != null)
                    NameTMPText.text = "NULL";
                if (SoloButton != null)
                {
                    SoloButton.interactable = false;
                }
                if (DisableButton != null)
                {
                    DisableButton.interactable = false;
                }
                return;
            }

            if (NameText != null)
                NameText.text = Feature.name;
            if (NameTMPText != null)
                NameTMPText.text = Feature.name;
            int FeatureIndex = StormDataCache.GetFeatureIndex(Feature);
            if (SoloButton != null)
            {
                SoloButton.isOn = World.Debug_SoloFeatureIdx == FeatureIndex;
                SoloButton.onValueChanged.AddListener(OnSoloToggle);
            }
            if (DisableButton != null)
            {
                DisableButton.isOn = World.SystemSettings.IsFeatureDisabled(Featureset, Feature);
                DisableButton.onValueChanged.AddListener(OnDisableToggle);
            }
        }
        public void OnSoloToggle(bool enabled)
        {
            int idx = StormDataCache.GetFeatureIndex(Feature);
            if (enabled)
                World.Debug_SoloFeatureIdx = idx;
            else if (World.Debug_SoloFeatureIdx == idx)
                World.Debug_SoloFeatureIdx = -1;
        }
        public void OnDisableToggle(bool disabled)
        {
            if (World.Debug_SoloFeatureIdx == StormDataCache.GetFeatureIndex(Feature))
                World.Debug_SoloFeatureIdx = -1;
            World.SystemSettings.SetFeatureDisabled(Featureset, Feature, disabled);
            if (SoloButton != null)
            {
                SoloButton.SetIsOnWithoutNotify(false);
                SoloButton.interactable = !disabled;
            }
            OnDisableChanged?.Invoke();
        }

    }
}