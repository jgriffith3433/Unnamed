using ProceduralWorlds.Storm.Utils;
using UnityEngine;
#if PW_STORM_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    public abstract class UIOnKeyBase : MonoBehaviour
    {
        public static bool DisableCapture = false;

        public string SettingName = "";

        public System.Action OnChanged;

        public abstract void OnPressed();
#if PW_STORM_INPUT_SYSTEM
        public InputAction Action;
        private void OnEnable()
        {
            Action.started -= OnToggle;
            Action.started += OnToggle;
            if (!string.IsNullOrEmpty(SettingName))
                Load();
            else
                Action.Enable();
            OnChanged?.Invoke();
        }
        private void OnDisable()
        {
            Action.started -= OnToggle;
            Action.Disable();
        }

        public void OnToggle(InputAction.CallbackContext context)
        {
            if (DisableCapture)
                return;
            OnPressed();
        }
#else
        public KeyCode Key;

        void Update()
        {
            if (!DisableCapture && Input.GetKeyDown(Key))
                OnPressed();
        }
#endif
        public void SetKey(KeyCode key)
        {
            SetKeyInternal(key);
            Save();
            OnChanged?.Invoke();
        }
        void SetKeyInternal(KeyCode key)
        {
#if PW_STORM_INPUT_SYSTEM
            Action.Disable();
            string path = StormUtils.KeyToInputPath(key);
            Action.ApplyBindingOverride(path);
            Action.Enable();
#else
            Key = key;
#endif
        }
        public KeyCode GetKey()
        {
#if PW_STORM_INPUT_SYSTEM
            if (Action.bindings.Count > 0)
            {
                string path = Action.bindings[0].overridePath;
                if (string.IsNullOrEmpty(path))
                    path = Action.bindings[0].path;
                return StormUtils.InputPathToKey(path);
            }
            return KeyCode.None;
#else
            return Key;
#endif
        }
        public string GetKeyString()
        {
#if PW_STORM_INPUT_SYSTEM
            return Action.GetBindingDisplayString();
#else
            return Key.ToString();
#endif
        }

        public void Save()
        {
            PlayerPrefs.SetString("STORM_KEY_" + SettingName, GetKey().ToString());
            PlayerPrefs.Save();
        }
        public void Load()
        {
            string val = PlayerPrefs.GetString("STORM_KEY_" + SettingName, GetKey().ToString());
            if (System.Enum.TryParse(val, out KeyCode key))
                SetKeyInternal(key);
#if PW_STORM_INPUT_SYSTEM
            else
                Action.Enable();
#endif
        }
    }
}