using System;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [ExecuteAlways]
    public class StormTransformList : MonoBehaviour
    {

#if UNITY_2021_2_OR_NEWER
        public ReadOnlySpan<StormUnityItemData> Items
        {
            get {
                return _items.AsSpan();
            }
        }
        public ReadOnlySpan<Transform> Transforms
        {
            get {
                return _transforms.AsSpan();
            }
        }
#else
        public StormUnityItemData[] Items
        {
            get {
                return _items;
            }
        }
        public Transform[] Transforms
        {
            get {
                return _transforms;
            }
        }
#endif
        [SerializeField]
        private StormUnityItemData[] _items = Array.Empty<StormUnityItemData>();
        [SerializeField]
        private Transform[] _transforms = Array.Empty<Transform>();
        public int ItemCount => _transforms.Length;

        public UnityEngine.Events.UnityAction<int> OnItemChanged;

        public Vector3 PositionOffset;
        public Vector3 PositionScaleXZ;

        [HideInInspector]
        [SerializeField]
        private bool m_isUpdated = false;

        private void OnEnable()
        {
            if (!m_isUpdated)
            {
                for (int i = 0; i < _items.Length; ++i)
                {
                    StormUnityItemData item = _items[i];
                    item.ExtraData.y = UnityEngine.Random.Range(0f, 1f);
                    _items[i] = item;
                    StormUtils.SetAssetDirty(this);
                }
                m_isUpdated = true;
            }
        }

        public StormUnityItemData GetItem(int idx)
        {
            return _items[idx];
        }

        public void AddItem(Transform t)
        {
            int numItems = _transforms.Length;
            for (int i = 0; i < numItems; i = i + 1)
            {
                if (_transforms[i] == t)
                {
                    UpdateTransform(i);
                    return;
                }
            }
            Array.Resize(ref _items, numItems + 1);
            Array.Resize(ref _transforms, numItems + 1);
            _transforms[numItems] = t;
            _items[numItems] = new StormUnityItemData(t, PositionOffset, PositionScaleXZ);
        }
        public void Clear()
        {
            Array.Resize(ref _items, 0);
            Array.Resize(ref _transforms, 0);
        }
        private int Find(Transform t)
        {
            int len = _transforms.Length;
            for (int i = 0; i < len; i = i + 1)
            {
                if (_transforms[i] == t)
                    return i;
            }
            return -1;
        }
        public void UpdateTransform(Transform t)
        {
            int idx = Find(t);
            if (idx < 0)
                AddItem(t);
            else
                UpdateTransform(idx);
        }
        public void UpdateTransform(int idx)
        {
            if (_transforms == null || idx >= _transforms.Length)
                return;
            if (idx >= 0)
            {
                if (_transforms[idx] != null)
                    _items[idx] = new StormUnityItemData(_transforms[idx], PositionOffset, PositionScaleXZ, _items[idx].ExtraData);
            }
            else
            {
                int numItems = _transforms.Length;
                if (numItems != _items.Length)
                    Array.Resize(ref _items, numItems);
                for (int i = 0; i < numItems; ++i)
                {
                    if (_transforms[i] != null)
                        _items[i] = new StormUnityItemData(_transforms[i], PositionOffset, PositionScaleXZ, new Vector2(0, (i % 10) / 10f));
                }
            }
            OnItemChanged?.Invoke(idx);
        }
        public void RemoveTransform(Transform t)
        {
            int idx = Find(t);
            if (idx >= 0)
                RemoveTransform(idx);
        }
        public void RemoveTransform(int idx)
        {
            int numItems = _transforms.Length - 1;
            if (numItems < 0)
                return;
            for (int i = idx; i < numItems; ++i)
            {
                _transforms[i] = _transforms[i + 1];
                _items[i] = _items[i + 1];
            }
            Array.Resize(ref _items, numItems);
            Array.Resize(ref _transforms, numItems);
            OnItemChanged?.Invoke(-1);
        }
    }
}