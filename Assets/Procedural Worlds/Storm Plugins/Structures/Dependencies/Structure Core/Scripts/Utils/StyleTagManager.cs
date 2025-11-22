using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Alternative approach: Singleton manager that loads style tags from Resources
/// This allows for data-driven configuration while maintaining global access
/// </summary>
public class StyleTagManager : MonoBehaviour
{
    private static StyleTagManager _instance;
    private static Dictionary<string, StyleTag> _styleTagLookup;
    private static List<StyleTag> _styleTags;
    
    /// <summary>
    /// Singleton instance
    /// </summary>
    public static StyleTagManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance
                _instance = FindObjectOfType<StyleTagManager>();
                
                // Create new instance if none exists
                if (_instance == null)
                {
                    GameObject go = new GameObject("StyleTagManager");
                    _instance = go.AddComponent<StyleTagManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Get all available style tags
    /// </summary>
    public static IReadOnlyList<StyleTag> StyleTags
    {
        get
        {
            EnsureInitialized();
            return _styleTags;
        }
    }
    
    /// <summary>
    /// Get a style tag by name
    /// </summary>
    /// <param name="name">The name of the style tag</param>
    /// <returns>The StyleTag if found, null otherwise</returns>
    public static StyleTag GetStyleTag(string name)
    {
        EnsureInitialized();
        _styleTagLookup.TryGetValue(name, out StyleTag styleTag);
        return styleTag;
    }
    
    /// <summary>
    /// Check if a style tag exists
    /// </summary>
    /// <param name="name">The name of the style tag</param>
    /// <returns>True if the style tag exists</returns>
    public static bool HasStyleTag(string name)
    {
        EnsureInitialized();
        return _styleTagLookup.ContainsKey(name);
    }
    
    /// <summary>
    /// Get materials for a specific style tag
    /// </summary>
    /// <param name="name">The name of the style tag</param>
    /// <returns>List of materials or empty list if not found</returns>
    public static List<Material> GetMaterials(string name)
    {
        var styleTag = GetStyleTag(name);
        return styleTag?.materials ?? new List<Material>();
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStyleTags();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private static void EnsureInitialized()
    {
        if (_styleTags == null || _styleTagLookup == null)
        {
            var instance = Instance; // This will create the instance if needed
            instance.InitializeStyleTags();
        }
    }
    
    private void InitializeStyleTags()
    {
        _styleTags = new List<StyleTag>();
        _styleTagLookup = new Dictionary<string, StyleTag>();
        
        // Option 1: Load from Resources (if you have ScriptableObject assets)
        // StyleTagDirectory[] directories = Resources.LoadAll<StyleTagDirectory>("StyleTags");
        // foreach (var directory in directories)
        // {
        //     foreach (var styleTag in directory.styleTags)
        //     {
        //         if (!string.IsNullOrEmpty(styleTag.name) && !_styleTagLookup.ContainsKey(styleTag.name))
        //         {
        //             _styleTags.Add(styleTag);
        //             _styleTagLookup[styleTag.name] = styleTag;
        //         }
        //     }
        // }
        
        // Option 2: Define style tags in code (hardcoded constants)
        DefineStyleTags();
    }
    
    private void DefineStyleTags()
    {
        // TODO: Define your style tags here
        // Example:
        // AddStyleTag("Wood", new List<Material> { /* your materials */ });
        // AddStyleTag("Stone", new List<Material> { /* your materials */ });
        // AddStyleTag("Metal", new List<Material> { /* your materials */ });
    }
    
    private void AddStyleTag(string name, List<Material> materials)
    {
        if (string.IsNullOrEmpty(name) || _styleTagLookup.ContainsKey(name))
            return;
            
        var styleTag = new StyleTag(name, materials);
        _styleTags.Add(styleTag);
        _styleTagLookup[name] = styleTag;
    }
}
