using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class StormEditorProgress
    {
        static string lastTitle = "Storm Progress";
        static string lastMessage = "";
        static string lastComment = "";
        static int lastTarget = 1;
        static int lastValue = 0;
        static int subTarget = 1;
        static float subValue = 0;
        static bool showProgress = true;
#if UNITY_EDITOR
        static double lastUpdateTS = 0d;
#endif

        public static double MinTimeBetweenUpdates { get; set; } = 0.2d;

        public static bool Cancelled { get; private set; } = true;
        public static string Title
        {
            get {
                return lastTitle;
            }
            set {
                lastTitle = value;
                DoProgress();
            }
        }
        public static string Message
        {
            get {
                return lastMessage;
            }
            set {
                lastMessage = value;
                DoProgress();
            }
        }
        public static string Comment
        {
            get {
                return lastComment;
            }
            set {
                lastComment = value;
                DoProgress();
            }
        }
        public static int Target
        {
            get {
                return lastTarget;
            }
            set {
                if (value < 1)
                    lastTarget = 1;
                else
                    lastTarget = value;
                DoProgress();
            }
        }
        public static int Value
        {
            get {
                return lastValue;
            }
            set {
                lastValue = value;
                DoProgress();
            }
        }
        public static bool ShowProgress
        {
            get {
                return showProgress;
            }
            set {
                showProgress = value;
                DoProgress();
            }
        }
        public static float SubValue
        {
            get {
                return subValue;
            }
            set {
                subValue = value;
                DoProgress();
            }
        }
        public static int SubTarget
        {
            get {
                return subTarget;
            }
            set {
                if (value < 0)
                    subTarget = 0;
                else
                    subTarget = value;
                DoProgress();
            }
        }

        public static bool Show(string title, string message, int Target, int Value = 0, string Comment = "")
        {
#if UNITY_EDITOR
            Clear();
            lastTitle = title;
            lastMessage = message;
            if (Target < 1)
                Target = 1;
            lastTarget = Target;
            lastValue = Value;
            lastComment = Comment;
#endif
            Cancelled = false;
            return DoProgress(true);
        }
        public static void Clear()
        {
            Cancelled = true;
#if UNITY_EDITOR
            lastTitle = "Storm Progress";
            lastMessage = "";
            lastComment = "";
            lastTarget = 1;
            lastValue = 0;
            subTarget = 1;
            subValue = 0;
            showProgress = true;
            EditorUtility.ClearProgressBar();
#endif
        }
        public static bool Update(int Value, bool force = false)
        {
#if UNITY_EDITOR
            lastValue = Value;
#endif
            return DoProgress(force);
        }
        public static bool Update(int Value, int Target, bool force = false)
        {
#if UNITY_EDITOR
            lastValue = Value;
            if (Target < 1)
                Target = 1;
            lastTarget = Target;
#endif
            return DoProgress(force);
        }
        public static bool Update(string Comment, int Value, bool force = false)
        {
#if UNITY_EDITOR
            lastComment = Comment;
            lastValue = Value;
#endif
            return DoProgress(force);
        }
        public static bool Update(string Comment, int Value, int Target, bool force = false)
        {
#if UNITY_EDITOR
            lastComment = Comment;
            lastValue = Value;
            if (Target < 1)
                Target = 1;
            lastTarget = Target;
#endif
            return DoProgress(force);
        }
        public static bool UpdateSubValue(int Value, bool force = false)
        {
#if UNITY_EDITOR
            subValue = Value;
#endif
            return DoProgress(force);
        }
        public static bool UpdateSubValue(int Value, int Target, bool force = false)
        {
#if UNITY_EDITOR
            subValue = Value;
            subTarget = Target;
#endif
            return DoProgress(force);
        }
        public static bool UpdateSubValue(string Comment, int Value, bool force = false)
        {
#if UNITY_EDITOR
            lastComment = Comment;
            subValue = Value;
#endif
            return DoProgress(force);
        }
        public static bool UpdateSubValue(string Comment, int Value, int Target, bool force = false)
        {
#if UNITY_EDITOR
            lastComment = Comment;
            subValue = Value;
            subTarget = Target;
#endif
            return DoProgress(force);
        }
        public static void ClearSubValue(bool advance = false, bool force = false)
        {
            subValue = 0;
            subTarget = 0;
            lastComment = "";
            if (advance)
                ++lastValue;
            DoProgress(force);
        }
        public static void ForceUpdate()
        {
            DoProgress(true);
        }

#if UNITY_EDITOR
        static float GetProgress()
        {
            if (subTarget > 0)
            {
                return Mathf.Clamp01(lastValue / (float)lastTarget + (subValue / (float)subTarget) * 1f / lastTarget);
            }
            return Mathf.Clamp01(lastValue / (float)lastTarget);
        }
        static string GetMessage()
        {
            string msg = lastMessage;
            if (ShowProgress)
                msg += " (" + lastValue + "/" + lastTarget + ")";
            if (!string.IsNullOrEmpty(lastComment))
                msg += ": " + lastComment;
            return msg;
        }
#endif

        static bool DoProgress(bool forceUpdate = false)
        {
#if UNITY_EDITOR
            if (!Cancelled)
            {
                double curTime = EditorApplication.timeSinceStartup;
                if (forceUpdate || curTime - lastUpdateTS > MinTimeBetweenUpdates)
                {
                    lastUpdateTS = curTime;
                    Cancelled = EditorUtility.DisplayCancelableProgressBar(lastTitle, GetMessage(), GetProgress());
                    if (Cancelled)
                        Clear();
                }
            }
#endif
            return Cancelled;
        }
    }
}
