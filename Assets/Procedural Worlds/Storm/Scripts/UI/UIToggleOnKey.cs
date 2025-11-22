using UnityEngine;
/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Toggle On Key")]
    public class UIToggleOnKey : UIOnKeyBase
    {
        public GameObject[] ToToggle = new GameObject[0];

        public override void OnPressed()
        {
            foreach (GameObject go in ToToggle)
                if (go != null)
                    go.SetActive(!go.activeSelf);
        }
    }
}