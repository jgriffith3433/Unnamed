# Quick Reference - Hybrid Video System

## Setup Checklist

- [ ] Canvas with RawImage created
- [ ] RenderTexture created and assigned to RawImage
- [ ] VideoPlayer GameObject created with video clip
- [ ] RenderTexture assigned to VideoPlayer's Target Texture
- [ ] Timeline with Playable Director created
- [ ] Hybrid Video Track added to Timeline
- [ ] Hybrid Video Clip added to track
- [ ] VideoPlayer and RenderTexture assigned in clip Inspector
- [ ] **"Extract Frames Automatically"** clicked
- [ ] Frames extracted and loaded successfully

## Key Settings

| Setting | Recommended Value | Notes |
|---------|------------------|-------|
| Preview FPS | 30 | Balance between quality and memory |
| Video Player - Play On Awake | OFF | Required for Timeline control |
| Video Player - Loop | OFF | Timeline controls playback |
| Video Player - Wait For First Frame | ON | Ensures smooth start |
| Video Player - Render Mode | Render Texture | Required |

## Inspector Workflow

```
No frames exist
    ↓
[Extract Frames Automatically] ← Click this
    ↓
Extracting... (progress bar)
    ↓
Frames loaded! ✓
    ↓
[Reload Frames] or [Re-Extract Frames] available
```

## Frame Location Formula

```
Video location: Assets/Videos/MyVideo.mp4
Frames location: Assets/Videos/MyVideo_frames/
                 ↓
                 frame_00000.asset
                 frame_00001.asset
                 ...
```

## Memory Calculation

```
Frames in memory = (Video Duration in seconds) × (Preview FPS)
Memory per frame ≈ (Width × Height × 3 bytes)

Example (1920×1080, 30fps, 60 seconds):
= 60 × 30 = 1800 frames
= 1920 × 1080 × 3 = 6,220,800 bytes per frame
= 1800 × 6.2MB ≈ 11GB total
```

💡 **Tip:** For longer videos, use lower FPS (15-24) to reduce memory usage!

## Common Issues - Quick Fixes

| Issue | Fix |
|-------|-----|
| Can't see Extract button | Assign VideoPlayer with video clip first |
| Frames not showing in Timeline | Click "Load Existing Frames" button |
| Video plays but no scrubbing | Frames not extracted - click Extract button |
| Extraction stuck | Check video is readable, try lower FPS |
| High memory usage | Lower Preview FPS, use shorter clips |

## Keyboard Shortcuts (Unity Timeline)

- **Space** - Play/Pause
- **Home** - Go to start
- **End** - Go to end
- **,** - Previous frame
- **.** - Next frame
- **Drag** - Scrub timeline (shows extracted frames!)

## File Structure

```
Assets/
  Scripts/
    HybridVideo/
      Runtime/
        HybridVideoPlayableAsset.cs      ← Main asset
        HybridVideoBehaviour.cs           ← Frame display logic
        HybridVideoTrack.cs               ← Timeline track
        HybridVideoMixerBehaviour.cs      ← Track mixer
      Editor/
        HybridVideoPlayableAssetEditor.cs ← Custom inspector (automatic!)
        VideoFrameExtractor.cs            ← Extraction engine
      README_AUTOMATIC.md                 ← Full documentation
      QUICK_REFERENCE.md                  ← This file
```

## Workflow Comparison

### OLD (Manual)
1. Open Tools → Video Frame Extractor
2. Assign VideoPlayer
3. Set FPS
4. Choose output path manually
5. Extract frames
6. Open Timeline clip
7. Manually assign all frame assets
8. Adjust array size

### NEW (Automatic) ✨
1. Select Timeline clip
2. Assign VideoPlayer and RenderTexture
3. Click "Extract Frames Automatically"
4. Done!

## Tips for Best Results

✓ **Use consistent FPS** - Stick with 24 or 30 for most projects
✓ **Organize videos** - Keep videos in dedicated folders
✓ **Name clearly** - Frames folder auto-named: `VideoName_frames`
✓ **Test first** - Extract a short clip first to test memory usage
✓ **Clean up** - Delete frame folders you no longer need
✓ **Backup** - Frame extraction takes time, back up extracted frames

## Performance Optimization

### For Long Videos (>5 minutes)
- Use 15-20 FPS preview
- Consider splitting into multiple clips
- Lower RenderTexture resolution

### For High Quality
- Use 30-60 FPS preview
- Match RenderTexture to video resolution
- Ensure sufficient RAM available

### For Fast Iteration
- Extract at low FPS first (15)
- Test your Timeline setup
- Re-extract at higher FPS when finalized
