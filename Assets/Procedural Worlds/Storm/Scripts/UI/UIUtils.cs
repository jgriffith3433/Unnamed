using UnityEngine;

namespace ProceduralWorlds.Storm.UI
{
    public static class UIUtils
    {
        //Helper to deal with the FindObjectOfType API changes
        public static T FindObjectOfType<T>(bool findInactive = false) where T : UnityEngine.Object
        {
#if UNITY_6000_0_OR_NEWER
            return GameObject.FindFirstObjectByType(typeof(T), findInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude) as T;
#else
		return GameObject.FindObjectOfType(typeof(T), findInactive) as T;
#endif
        }
        //Helper to deal with the FindObjectsOfType API changes
        public static T[] FindObjectsOfType<T>(bool findInactive = false) where T : UnityEngine.Object
        {
#if UNITY_6000_0_OR_NEWER
            return GameObject.FindObjectsByType(typeof(T), findInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None) as T[];
#else
        return GameObject.FindObjectsOfType(typeof(T),findInactive) as T[];
#endif
        }
    }
}