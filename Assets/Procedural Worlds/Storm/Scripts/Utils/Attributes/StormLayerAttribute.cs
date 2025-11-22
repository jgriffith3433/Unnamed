using System;
using UnityEngine;

namespace ProceduralWorlds.Storm.Utils
{
    public enum StormLayerRemapMode
    {
        DATA,
        FEATURESET,
        HIDE
    }
    /// <summary>
    /// Specifies that a field represents a storm layer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class StormLayerAttribute : PropertyAttribute
    {
        public readonly string title;
        public readonly StormLayerRemapMode mode;

        public StormLayerAttribute()
        {
            this.title = null;
            this.mode = StormLayerRemapMode.DATA;
        }
        public StormLayerAttribute(string title, StormLayerRemapMode mode = StormLayerRemapMode.DATA)
        {
            this.title = title;
            this.mode = mode;
        }
    }
}