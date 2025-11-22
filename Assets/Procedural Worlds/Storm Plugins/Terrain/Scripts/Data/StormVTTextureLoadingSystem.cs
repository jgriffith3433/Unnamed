using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using ProceduralWorlds.Storm.Terrain;
using static ProceduralWorlds.Storm.Terrain.StormVTEnums;

public enum TextureType
{
    Height = 0,
    Normal = 1,
    Splat = 2,
    Hole = 3,
    Image = 4
}

public struct TextureCallback
{
    public TextureType type;
    public int index;
    public Texture2D texture;
}

public struct TextureLoadRequest
{
    public string ID;
    public string[] FilePaths;
    public TextureType[] TextureTypes;
    public TerrainRequest[] TerrainData;
    public Action<List<TextureCallback>> OnLoaded;
}

public struct TerrainRequest
{
    public string FilePath;
    public TextureType TextureType;
    public int index;
}

public class StormVTTextureLoadingSystem : MonoBehaviour
{
    /// <summary>
    /// Reference to the storm terrain settings
    /// </summary>
    public StormVTSettings stormVTSettings;

    /// <summary>
    /// The amount of the parallel tasks to perform when loading a texture
    /// </summary>
    [HideInInspector]
    [Range(0, 20)] [SerializeField] public int m_maxParallelTasks = 4;

    /// <summary>
    /// The size of each parallel chunk (in bytes) to read from a texture
    /// </summary>
    [HideInInspector]
    [SerializeField] public int m_chunkSize = 4096;

    /// <summary>
    /// The amount of the parallel chunks to perform when loading a texture
    /// </summary>
    [HideInInspector]
    [Range(1, 20)] [SerializeField] public int m_maxParallelChunks = 2;

    /// <summary>
    /// Cancellation token to make sure that texture loading will stop when the operation is preemptively stopped
    /// </summary>
    private CancellationTokenSource cancellationTokenSource;

    /// <summary>
    /// Queue of requests that need to be processed
    /// </summary>
    private DictionaryQueue<string, TextureLoadRequest> requestQueue = new DictionaryQueue<string, TextureLoadRequest>();

    /// <summary>
    /// Queue of current requests that are being processed
    /// </summary>
    private DictionaryQueue<string, TextureLoadRequest> processQueue = new DictionaryQueue<string, TextureLoadRequest>();

    /// <summary>
    /// The amount of the parallel tasks to perform when loading a texture
    /// </summary>
    [HideInInspector]
    public int MaxParallelTasks
    {
        get => m_maxParallelTasks;
        set => m_maxParallelTasks = value;
    }

    /// <summary>
    /// The size of each parallel chunk (in bytes) to read from a texture
    /// </summary>
    [HideInInspector]
    public int ChunkSize
    {
        get => m_chunkSize;
        set => m_chunkSize = value;
    }

    /// <summary>
    /// The amount of the parallel chunks to perform when loading a texture
    /// </summary>
    [HideInInspector]
    public int MaxParallelChunks
    {
        get => m_maxParallelChunks;
        set => m_maxParallelChunks = value;
    }

    private bool m_isInitialized = false;
    AsyncImageLoader.LoaderSettings loaderSettings;

    [HideInInspector]
    public bool runningRequest = false;

    public void Start()
    {
        StormVTSettings.OnUpdateStormVTSettings += StormSettingsUpdated;
        StormSettingsUpdated();
    }

    /// <summary>
    /// Update the values when the storm terrain settings changes
    /// </summary>
    public void StormSettingsUpdated()
    {
        if(stormVTSettings != null)
        {
            if (stormVTSettings.m_stormVTMode == StormVTMode.Clipmap)
            {
                if (!m_isInitialized)
                {
                    InitializeSystem();
                }
                ChunkSize = stormVTSettings.m_chunkSize;
                MaxParallelChunks = stormVTSettings.m_maxParallelChunks;
                MaxParallelTasks = stormVTSettings.m_maxParallelTasks;
            }
            else if (m_isInitialized)
            {
                CleanupSystem();
            }
        }
    }

    private void InitializeSystem()
    {
        cancellationTokenSource = new CancellationTokenSource();
        Texture.allowThreadedTextureCreation = true;
        
        loaderSettings = AsyncImageLoader.LoaderSettings.Default;
        loaderSettings.generateMipmap = false;
        loaderSettings.linear = true;
        loaderSettings.markNonReadable = true;

        m_isInitialized = true;
    }

    private void CleanupSystem()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        foreach(var request in requestQueue.Values)
        {
            CancelRequest(request.ID);
        }
        requestQueue.Clear();
        processQueue.Clear();
        runningRequest = false;

        m_isInitialized = false;
    }

#if UNITY_ANDROID
async void DoUpdate()
#else
    async void Update()
#endif
    {
        if(!m_isInitialized)
            return;
            
        if (requestQueue.Count > 0 && !runningRequest)
        {
            KeyValuePair<string, TextureLoadRequest> pair = requestQueue.Dequeue();
            TextureLoadRequest request = pair.Value;
            string requestID = request.ID;

            // Add to Process Queue
            processQueue.Add(requestID, request);

            runningRequest = true;

            List<TextureCallback> textureCallbacks = new List<TextureCallback>();

            foreach (TerrainRequest terrainData in request.TerrainData)
            {
                string filePath = terrainData.FilePath;
                if (!File.Exists(filePath))
                    continue;

                if (filePath.EndsWith(".png") ||
                    filePath.EndsWith(".jpg") ||
                    filePath.EndsWith(".jpeg"))
                {
                    TextureResult result = await LoadImageAsync(requestID, filePath);
                    if (result.isSuccessful)
                    {
                        string splatIndex =  (terrainData.TextureType == TextureType.Splat ? "_" + terrainData.index.ToString() : "");
                        result.texture2D.name = $"{requestID}_{terrainData.TextureType}{splatIndex}";
                        textureCallbacks.Add(new TextureCallback()
                        {
                            type = terrainData.TextureType,
                            texture = result.texture2D,
                            index = terrainData.index
                        });
                    }
                }
                else
                {
                    Debug.LogWarning($"File not found or invalid format: {filePath}");
                }
            }
            
            if (textureCallbacks.Count > 0)
            {
                request.OnLoaded?.Invoke(textureCallbacks);
                runningRequest = false;
            }
            
            // Remove from Process Queue
            processQueue.Remove(requestID);
        }
    }

    public void RequestTextureLoad(string id, TerrainRequest[] terrainDatas, Action<List<TextureCallback>> onLoaded)
    {
        if (!requestQueue.ContainsKey(id) && !processQueue.ContainsKey(id))
        {
            TextureLoadRequest request = new TextureLoadRequest
            {
                ID = id,
                TerrainData = terrainDatas,
                OnLoaded = onLoaded
            };
            requestQueue.Enqueue(id, request);
        }
    }

    public void CancelRequest(string id)
    {
        // If the request is currently in the request queue
        if (requestQueue.ContainsKey(id))
        {
            // Remove it from the Quest Queue
            requestQueue.Remove(id);
        }
        // Otherwise, it's probably currently processing
        else
        {
            //If it is currently running
            if (processQueue.ContainsKey(id))
            {
                // Cancel it!
                processQueue.Remove(id); 
            }
        }
    }

    public struct TextureResult
    {
        public static TextureResult Failure = new TextureResult(false, null);
        
        public bool isSuccessful;
        public Texture2D texture2D;

        public TextureResult(bool isSuccessful, Texture2D texture2D)
        {
            this.isSuccessful = isSuccessful;
            this.texture2D = texture2D;
        }
    }
    
    // Change LoadImageAsync to return Texture2D
    private async Task<TextureResult> LoadImageAsync(string id, string imagePath)
    {
        cancellationTokenSource.Token.ThrowIfCancellationRequested();

        ByteResult result = await ReadFileInChunksAsync(id, imagePath, m_chunkSize, m_maxParallelChunks);
        if (!result.isSuccessful)
            return TextureResult.Failure;

        byte[] imageData = result.data;


        Texture2D texture = GetTexture2D();
        
        // No longer in Process Queue?
        if (!processQueue.ContainsKey(id))
            return TextureResult.Failure;

        bool success = await AsyncImageLoader.LoadImageAsync(texture, imageData, loaderSettings);
        
        // No longer in Process Queue?
        if (!processQueue.ContainsKey(id))
            return TextureResult.Failure;
        
        return new TextureResult(success, success ? texture : null);
    }

    public struct ByteResult
    {
        public static ByteResult Failure = new ByteResult(false, null);
        
        public bool isSuccessful;
        public byte[] data;

        public ByteResult(bool isSuccessful, byte[] data)
        {
            this.isSuccessful = isSuccessful;
            this.data = data;
        }
    }

    private async Task<ByteResult> ReadFileInChunksAsync(string id, string filePath, int chunkSize, int maxParallelChunks)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            long fileLength = fileInfo.Length;
            byte[] imageData = new byte[fileLength];

            // Increase minimum chunk size to reduce number of operations
            int adjustedChunkSize = Math.Max(chunkSize, 32768); // Minimum 32KB chunks
            int chunkCount = (int)math.ceil((double)fileLength / adjustedChunkSize);

            // Create a reusable list of tasks to avoid allocations
            List<Task> tasks = new List<Task>(maxParallelChunks);
            using (SemaphoreSlim chunkSemaphore = new SemaphoreSlim(maxParallelChunks))
            {
                for (int i = 0; i < chunkCount; i++)
                {
                    // No longer in the Process Queue?
                    if (!processQueue.ContainsKey(id))
                    {
                        Debug.LogWarning($"Request {id} no longer in process queue, aborting file read");
                        return ByteResult.Failure;
                    }

                    // Calculate chunk boundaries
                    int offset = i * adjustedChunkSize;
                    int size = (int)math.min(adjustedChunkSize, fileLength - offset);

                    // Wait for semaphore with timeout to prevent deadlocks
                    if (!await chunkSemaphore.WaitAsync(1000, cancellationTokenSource.Token))
                    {
                        Debug.LogWarning($"Timeout waiting for chunk semaphore in file: {filePath}");
                        return ByteResult.Failure;
                    }

                    try
                    {
                        // Add the task to our list
                        tasks.Add(ReadAndCopyChunkAsync(filePath, new byte[size], imageData, offset, size, chunkSemaphore));
                        
                        // When we have enough tasks or this is the last chunk, wait for completion
                        if (tasks.Count >= maxParallelChunks || i == chunkCount - 1)
                        {
                            await Task.WhenAll(tasks);
                            tasks.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error reading file chunk: {e.Message} in file: {filePath}");
                        return ByteResult.Failure;
                    }

                    // No longer in the Process Queue?
                    if (!processQueue.ContainsKey(id))
                    {
                        Debug.LogWarning($"Request {id} no longer in process queue after chunk read, aborting");
                        return ByteResult.Failure;
                    }
                }
            }
            
            return new ByteResult(true, imageData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in ReadFileInChunksAsync: {e.Message} for file: {filePath}");
            return ByteResult.Failure;
        }
    }

    private async Task ReadAndCopyChunkAsync(string filePath, byte[] buffer, byte[] target, int offset, int size,
        SemaphoreSlim chunkSemaphore)
    {
        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                int bytesRead = await fileStream.ReadAsync(buffer, 0, size);
                if (bytesRead < size)
                {
                    Debug.LogWarning($"Incomplete read at offset {offset}: expected {size} bytes but got {bytesRead}");
                }
                Buffer.BlockCopy(buffer, 0, target, offset, bytesRead);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in ReadAndCopyChunkAsync: {e.Message} at offset {offset} in file: {filePath}");
            throw; // Rethrow to be caught by the calling method
        }
        finally
        {
            chunkSemaphore.Release();
        }
    }

    private void OnDisable()
    {
        if (m_isInitialized)
        {
            CleanupSystem();
        }
        StormVTSettings.OnUpdateStormVTSettings -= StormSettingsUpdated;
    }

    private Texture2D GetTexture2D()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
        return texture;
    }
}