using UnityEngine;

public class URLOpener : MonoBehaviour
{
    /// <summary>
    /// Opens a URL in the default browser.
    /// Can be called from UI Button onClick events.
    /// </summary>
    /// <param name="url">The URL to open</param>
    public void OpenURL(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("URLOpener: URL is null or empty!");
            return;
        }

        Debug.Log($"Opening URL: {url}");
        Application.OpenURL(url);
    }
}
