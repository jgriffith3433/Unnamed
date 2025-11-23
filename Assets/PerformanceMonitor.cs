using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceMonitor : MonoBehaviour
{
    [SerializeField] private bool showStats = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F3;
    
    private float deltaTime = 0.016f;
    private GUIStyle style;
    private float[] frameTimes = new float[60];
    private int frameIndex = 0;
    
    void Start()
    {
        style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = Color.white;
        
        for (int i = 0; i < frameTimes.Length; i++)
        {
            frameTimes[i] = 0.016f;
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showStats = !showStats;
        }
        
        frameTimes[frameIndex] = Time.unscaledDeltaTime;
        frameIndex = (frameIndex + 1) % frameTimes.Length;
        
        float total = 0f;
        for (int i = 0; i < frameTimes.Length; i++)
        {
            total += frameTimes[i];
        }
        deltaTime = total / frameTimes.Length;
    }
    
    void OnGUI()
    {
        if (!showStats) return;
        
        int w = Screen.width;
        int h = Screen.height;
        
        Rect rect = new Rect(10, 10, 300, 150);
        
        // Background
        GUI.Box(rect, "");
        
        // Calculate stats
        float fps = 1.0f / deltaTime;
        float ms = deltaTime * 1000.0f;
        long totalMem = Profiler.GetTotalAllocatedMemoryLong() / 1048576;
        long monoMem = Profiler.GetMonoUsedSizeLong() / 1048576;
        
        // Display text
        string text = $"FPS: {fps:0.}\n";
        text += $"MS: {ms:0.0}\n";
        text += $"Total Memory: {totalMem} MB\n";
        text += $"Mono Memory: {monoMem} MB\n";
        text += $"\nPress {toggleKey} to toggle";
        
        GUI.Label(new Rect(15, 15, 290, 140), text, style);
    }
}
