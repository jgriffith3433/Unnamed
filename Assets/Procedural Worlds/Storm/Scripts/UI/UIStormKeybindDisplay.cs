using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using ProceduralWorlds.Storm.Utils;
#if PW_STORM_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Keybind Display")]
    public class UIStormKeybindDisplay : MonoBehaviour
#if PW_STORM_INPUT_SYSTEM
        , System.IObserver<InputControl>
#endif
    {
        public UIOnKeyBase ActionTarget;

        public TMP_Text KeyText;
        public Button SetButton;
        public GameObject CancelDialogParent;
        public Button CancelButton;

        bool RebindingKey = false;
#if PW_STORM_INPUT_SYSTEM
        IDisposable inputSubscription = null;
#endif

        public string SetKeyLabel = "Press any key";

        private void Start()
        {
            if (SetButton != null)
                SetButton.onClick.AddListener(OnBindButtonPress);
            if (CancelButton != null)
                CancelButton.onClick.AddListener(OnCancelButtonPress);
            ResetDisplays();
            if (ActionTarget != null)
            {
                ActionTarget.OnChanged -= ResetDisplays;
                ActionTarget.OnChanged += ResetDisplays;
            }
        }
        private void ResetDisplays()
        {
            RebindingKey = false;
            if (KeyText != null)
                KeyText.text = ActionTarget.GetKeyString();
            if (CancelDialogParent != null)
                CancelDialogParent.SetActive(false);
        }
        void OnBindButtonPress()
        {
            if (RebindingKey)
                return;
            if (KeyText != null)
                KeyText.text = SetKeyLabel;
            if (CancelDialogParent != null)
            {
                CancelDialogParent.transform.SetAsLastSibling();
                CancelDialogParent.SetActive(true);
            }
            RebindingKey = true;
            UIOnKeyBase.DisableCapture = true;
#if PW_STORM_INPUT_SYSTEM
            inputSubscription = InputSystem.onAnyButtonPress.Subscribe(this);
#endif
            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
        void OnCancelButtonPress()
        {
            if (!RebindingKey)
                return;
            ResetDisplays();
            StartCoroutine(CoroDelayedEnableBindings());
        }
        void SetValue(KeyCode key)
        {
            ActionTarget.SetKey(key);
            ResetDisplays();
        }

#if PW_STORM_INPUT_SYSTEM
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(InputControl value)
        {
            string path = value.path.ToLowerInvariant();
            if (path.Contains("keyboard"))
            {
                KeyCode key = StormUtils.InputPathToKey(path);
                if (key == KeyCode.Escape)
                    ResetDisplays();
                else if (key != KeyCode.None)
                    SetValue(key);
                inputSubscription.Dispose();
                StartCoroutine(CoroDelayedEnableBindings());
            }
        }
#else
        private void Update()
        {
            if (RebindingKey && Input.anyKeyDown)
            {
                KeyCode curKey = GetCurrentKey();
                if (curKey != KeyCode.None)
                {
                    if (curKey == KeyCode.Escape)
                        OnCancelButtonPress();
                    else
                        SetValue(curKey);
                }
            }
        }

        KeyCode GetCurrentKey()
        {
            foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (key != KeyCode.None && Input.GetKeyDown(key))
                    return key;
            }
            return KeyCode.None;
        }
#endif
        IEnumerator CoroDelayedEnableBindings()
        {
            yield return new WaitForEndOfFrame();
            UIOnKeyBase.DisableCapture = false;
        }
    }
}