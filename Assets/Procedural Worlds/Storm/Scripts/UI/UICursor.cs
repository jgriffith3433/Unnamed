using System.Collections.Generic;
using UnityEngine;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    /*
        UI Cursor - set to be the single point of access to set cursors to reduce conflicts between multiple scripts requesting cursors
            - cursors passed are indexes of cursors set in UICursor.Cursors
            - known CallerIDs:
                9043 - UIResizeHandle
            - UI scripts use priority 9001 for lock priority
    */
    [AddComponentMenu("Procedural Worlds/Storm/UI/Cursor")]
    [ExecuteAlways]
    public class UICursor : MonoBehaviour
    {
        [System.Serializable]
        public class UIMouseCursor
        {
            public string Name;
            public Texture2D cursor;
            public Vector2 hotspot;
        }
        public UIMouseCursor[] Cursors;
        public CursorSelection DefaultCursor;
        public CursorSelection ResizeVerticalCursor;
        public CursorSelection ResizeHorizontalCursor;
        public CursorSelection ResizeDiagonalTLCursor;
        public CursorSelection ResizeDiagonalTRCursor;

        private static UICursor _instance = null;
        public static UICursor Instance {
            get {
                if (_instance == null)
                    _instance = UIUtils.FindObjectOfType<UICursor>();
                return _instance;
            }
            private set => _instance = value;
        }
        private void Start()
        {
            Instance = this;
        }
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        //default cursor to use when no other cursors are set (default -1 pointer)
        private static int curCursor;
        private const int CursorLockedCode = -999;
        [System.Serializable]
        public struct CursorSelection
        {
            public int cursor;
            public int priority;
            public CursorSelection(int cur = -1, int pri = 50)
            {
                cursor = cur;
                priority = pri;
            }
        }
        private static Dictionary<int, CursorSelection> cursors = new Dictionary<int, CursorSelection>();

        //sets cursor to requested cursor provided there isn't a cursor set of higher priority.
        //cursor is index of requested cursor in UISettings.Cursors (negative numbers = pointer / no cursor)
        //CallerID is used to prevent overlap between scripts. number doesn't matter as long as it is unique.
        //higher priority takes ... priority
        public static void Set(int cursor, int CallerID, int priority = 0)
        {
            cursors.Remove(CallerID);
            CursorSelection cur;
            cur.cursor = cursor;
            cur.priority = priority;
            cursors.Add(CallerID, cur);
            SelectCursor();
        }
        public static void Set(CursorSelection cursor, int CallerID)
        {
            cursors.Remove(CallerID);
            CursorSelection cur = cursor;
            cursors.Add(CallerID, cur);
            SelectCursor();
        }
        public static void Hide(int CallerID, int priority = 0)
        {
            cursors.Remove(CallerID);
            CursorSelection cur;
            cur.cursor = CursorLockedCode;
            cur.priority = priority;
            cursors.Add(CallerID, cur);
            SelectCursor();
        }
        //clears cursor setting with matching CallerID
        public static void Clear(int CallerID)
        {
            cursors.Remove(CallerID);
            SelectCursor();
        }

        private static void SelectCursor()
        {
            if (Instance == null)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }
            if (cursors.Count == 0)
            {
                //no cursors so just set the default (arrow)
                if (Instance != null && Instance.DefaultCursor.cursor >= 0 && Instance.DefaultCursor.cursor < Instance.Cursors.Length)
                {
                    Cursor.SetCursor(Instance.Cursors[Instance.DefaultCursor.cursor].cursor, Vector2.zero, CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
            int bestCursor = Instance.DefaultCursor.cursor;
            int bestPriority = int.MinValue;
            foreach (KeyValuePair<int, CursorSelection> cur in cursors)
            {
                if (cur.Value.priority == bestPriority)
                {
                    //same priority so dont overload if it has a invalid cursor
                    if (cur.Value.cursor >= 0 && cur.Value.cursor < Instance.Cursors.Length)
                    {
                        bestCursor = cur.Value.cursor;
                    }
                }
                else if (cur.Value.priority > bestPriority)
                {
                    bestPriority = cur.Value.priority;
                    bestCursor = cur.Value.cursor;
                }
            }
            //handle hide/unhide
            if (bestCursor == CursorLockedCode)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (bestCursor < 0 || bestCursor >= Instance.Cursors.Length)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(Instance.Cursors[bestCursor].cursor, Instance.Cursors[bestCursor].hotspot, CursorMode.Auto);
            }
        }
    }
}