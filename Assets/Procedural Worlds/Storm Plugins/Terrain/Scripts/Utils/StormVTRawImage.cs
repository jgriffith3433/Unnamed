using ProceduralWorlds.Storm.Terrain;
using UnityEngine;
using UnityEngine.UI;

public class GetColorImage : MonoBehaviour
{

    #region Public Variables
    /// <summary>
    /// Reference to the UI Raw Image component
    /// </summary>
    public RawImage rawImage;

    /// <summary>
    /// Reference to the storm terrain manager component
    /// </summary>
    public StormVTPhysicalTextureManager manager;

    /// <summary>
    /// Reference to the feedback manager component
    /// </summary>
    public StormVTFeedbackManager feedback;

    /// <summary>
    /// Type of image to debug
    /// </summary>
    public StormVTEnums.StormVTRawImageDebugOption textureType;

    #endregion

    /// <summary>
    /// Subscribes to events
    /// </summary>
    public void SubscribeEvents()
    {
        StormVTPhysicalTextureManager.OnPhysicalTexturesCreated += SetPhysicalTexture;
        StormVTFeedbackRendererCustomPass.OnFeedbackTexturesCreated += SetPhysicalTexture;
    }

    /// <summary>
    /// Unsubscribes to events
    /// </summary>
    public void UnSubscribeEvents()
    {
        StormVTPhysicalTextureManager.OnPhysicalTexturesCreated -= SetPhysicalTexture;
        StormVTFeedbackRendererCustomPass.OnFeedbackTexturesCreated -= SetPhysicalTexture;
    }


    /// <summary>
    /// find the Storm Terrain components
    /// </summary>
    void Start()
    {
        rawImage = GetComponent<RawImage>();
        manager = StormVTUtilities.FindObjectByType<StormVTPhysicalTextureManager>();
        feedback = StormVTUtilities.FindObjectByType<StormVTFeedbackManager>();
        SubscribeEvents();
    }

    /// <summary>
    /// Unsubscribe from events when turned off
    /// </summary>
    private void OnDisable()
    {
        UnSubscribeEvents();
    }

    /// <summary>
    /// Assign to the raw image the corressponding phyiscal texture to display in the UI
    /// </summary>
    public void SetPhysicalTexture()
    {
        if (manager != null)
        {
            switch (textureType)
            {
                case StormVTEnums.StormVTRawImageDebugOption._Height:
                    rawImage.texture = manager.m_physicalTextureHeight;
                    break;
                case StormVTEnums.StormVTRawImageDebugOption._Normal:
                    rawImage.texture = manager.m_physicalTextureNormal;
                    break;
                case StormVTEnums.StormVTRawImageDebugOption._ColorSource0:
                    rawImage.texture = manager.m_physicalTextureColorSource0;
                    break;
                case StormVTEnums.StormVTRawImageDebugOption._ColorSource1:
                    rawImage.texture = manager.m_physicalTextureColorSource1;
                    break;
                case StormVTEnums.StormVTRawImageDebugOption._Feedback:
                    rawImage.texture = feedback.feedbackRendererCustomPass.feedbackRenderTexture;
                    break;
            }
        }

    }

    /// <summary>
    /// Check if somehow the reference to the manager or texture is lost, and if so, reassign the data
    /// </summary>
    private void Update()
    {
        if (manager == null)
        {
            manager = StormVTUtilities.FindObjectByType<StormVTPhysicalTextureManager>();
        }

        if (rawImage.texture == null && manager != null)
        {
            SetPhysicalTexture();

        }
    }

}
