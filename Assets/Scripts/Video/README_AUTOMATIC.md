# Hybrid Video System - Automatic Setup Guide

## Overview

The Hybrid Video System provides seamless video scrubbing in Unity Timeline's Edit Mode and full video playback in Play Mode. **All frame extraction is now automatic** - just assign a VideoPlayer and click a button!

## Installation

1. **Scripts are already in place:**
   - `Assets/Scripts/HybridVideo/Runtime/` - Runtime scripts
   - `Assets/Scripts/HybridVideo/Editor/` - Editor scripts

## Quick Setup (5 Steps)

### 1. Create Video Display

1. Create a **Canvas** (UI → Canvas)
2. Add a **RawImage** as child of Canvas
3. Create a **RenderTexture** asset:
   - Right-click in Project → Create → Render Texture
   - Set resolution to match your video (e.g., 1920x1080)
   - Name it `VideoRenderTexture`
4. Assign the RenderTexture to the RawImage's **Texture** field

### 2. Setup VideoPlayer

1. Create an empty GameObject named "VideoPlayer"
2. Add **VideoPlayer** component
3. Configure VideoPlayer:
   - **Source:** Video Clip or URL
   - **Render Mode:** Render Texture
   - **Target Texture:** Assign your VideoRenderTexture
   - **Play On Awake:** OFF
   - **Loop:** OFF
   - **Wait For First Frame:** ON

### 3. Create Timeline

1. Create an empty GameObject named "Timeline"
2. Add **Playable Director** component
3. Click **Create** to create a new Timeline asset
4. Open the Timeline window (Window → Sequencing → Timeline)

### 4. Add Hybrid Video Track

1. In Timeline, click **+** → **Hybrid Video Track**
2. Right-click on the track → **Add Hybrid Video Clip**
3. Adjust clip length to match video duration

### 5. Configure and Extract (Automatic!)

1. Select the clip in Timeline
2. In the Inspector:
   - **Video Player:** Assign your VideoPlayer GameObject
   - **Render Texture:** Assign your VideoRenderTexture
   - **Preview FPS:** Set to desired frame rate (30 recommended)
3. Click **"Extract Frames Automatically"** button
4. Wait for extraction to complete (progress bar shown)
5. Done! Frames are automatically loaded and ready to use

## How It Works

### Automatic Frame Management
- Frames are extracted to a folder next to your video file
- Folder name: `[VideoName]_frames`
- Frames are automatically loaded when needed
- No manual folder selection or frame assignment required

### Edit Mode (Timeline Scrubbing)
- Scrub the Timeline - frames display instantly
- Preloaded Texture2D array provides smooth preview
- No video playback occurs in Edit Mode

### Play Mode
- Press Play for full video playback
- VideoPlayer syncs automatically with Timeline
- Audio plays normally (if configured)
- Timeline controls video time

## Inspector Buttons Explained

**"Extract Frames Automatically"**
- Appears when no frames exist
- Extracts frames to automatic location next to video
- Automatically loads frames when complete

**"Load Existing Frames"**
- Appears when frames exist but aren't loaded
- Instantly loads all frames from the automatic location

**"Reload Frames"**
- Reloads frames from disk (useful if frames were modified)

**"Re-Extract Frames"**
- Deletes existing frames and extracts fresh ones
- Useful if you changed Preview FPS or video quality

## Performance Tips

- **Lower FPS (15-24):** Less memory usage, good for long videos
- **Higher FPS (30-60):** Smoother scrubbing, better for short videos
- **Memory:** Each frame is ~2-8MB depending on resolution
- Example: 30 FPS × 60 seconds × 4MB = ~7.2GB for a 1-minute 1080p video

## Multiple Videos

Want multiple videos in your Timeline?
1. Add multiple Hybrid Video Tracks
2. Each track has its own VideoPlayer and RenderTexture
3. Frames are automatically managed per video
4. Each video extracts to its own `_frames` folder

## Cinemachine Integration

Works seamlessly with Cinemachine:
- Add Cinemachine tracks alongside video tracks
- Videos sync with camera cuts and movements
- Use markers to align timing

## Advanced: Manual Frame Extraction

If you prefer the old manual method:
1. Go to **Tools → Video Frame Extractor**
2. Configure manually and extract
3. Frames still work with Timeline clips

## Troubleshooting

**"Assign a VideoPlayer with a video clip"**
- Make sure VideoPlayer has a video clip assigned
- Ensure the Timeline's Playable Director is in the scene

**Frames not showing in Edit Mode**
- Click "Load Existing Frames" if they exist
- Otherwise click "Extract Frames Automatically"
- Check that RenderTexture is assigned

**Video not playing in Play Mode**
- Verify VideoPlayer is assigned to the clip
- Ensure "Play On Awake" is OFF on VideoPlayer
- Check that RenderTexture is assigned to VideoPlayer

**Timeline desync**
- VideoPlayer auto-syncs within 0.1s tolerance
- This is intentional to prevent constant seeking
- Tighten tolerance in `HybridVideoBehaviour.cs` if needed

**Performance issues in Edit Mode**
- Reduce Preview FPS (try 15 or 24)
- Use lower resolution RenderTexture
- Consider shorter video clips

**Extraction takes too long**
- Lower Preview FPS reduces extraction time
- Extraction happens once - subsequent loads are instant
- Close other Unity windows during extraction

## Frame Storage Location

Frames are stored in:
```
Assets/
  YourVideoFolder/
    YourVideo.mp4
    YourVideo_frames/          ← Automatically created
      frame_00000.asset
      frame_00001.asset
      ...
```

This keeps frames organized next to their source video!

## What's New (Automatic Version)

✓ No manual frame extraction window needed
✓ Frames auto-detect and load
✓ Smart folder placement next to video files
✓ One-click extraction from Inspector
✓ Automatic frame count detection
✓ Visual status indicators
✓ Re-extraction support

## Support

For issues or questions:
1. Check the Troubleshooting section
2. Verify all setup steps were followed
3. Check Unity Console for error messages
4. Ensure video codec is supported by Unity
