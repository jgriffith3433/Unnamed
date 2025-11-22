using UnityEngine;
using UnityEngine.Events;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Set Inverse")]
    public class UISetInverse : MonoBehaviour
    {
        public UnityEvent<bool> OnSet;
        public void SetValue(bool val)
        {
            OnSet?.Invoke(!val);
        }
    }
}