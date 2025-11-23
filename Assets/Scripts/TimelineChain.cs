using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

public class TimelineChain : MonoBehaviour
{
    [SerializeField] private List<PlayableDirector> timelineSequence = new List<PlayableDirector>();
    [SerializeField] private bool playOnStart = true;
    
    private int currentTimelineIndex = 0;

    private void Start()
    {
        if (playOnStart && timelineSequence.Count > 0)
        {
            PlayTimeline(0);
        }
    }

    private void OnEnable()
    {
        // Subscribe to all timelines
        foreach (var timeline in timelineSequence)
        {
            if (timeline != null)
            {
                timeline.stopped += OnTimelineStopped;
            }
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from all timelines
        foreach (var timeline in timelineSequence)
        {
            if (timeline != null)
            {
                timeline.stopped -= OnTimelineStopped;
            }
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        // Check if this is the current timeline in the sequence
        if (currentTimelineIndex < timelineSequence.Count && 
            timelineSequence[currentTimelineIndex] == director)
        {
            // Move to next timeline
            currentTimelineIndex++;
            
            if (currentTimelineIndex < timelineSequence.Count)
            {
                PlayTimeline(currentTimelineIndex);
            }
            else
            {
                Debug.Log("Timeline sequence completed!");
                OnSequenceComplete();
            }
        }
    }

    private void PlayTimeline(int index)
    {
        if (index >= 0 && index < timelineSequence.Count && timelineSequence[index] != null)
        {
            Debug.Log($"Playing timeline {index + 1} of {timelineSequence.Count}");
            timelineSequence[index].Play();
        }
    }

    /// <summary>
    /// Manually start the timeline sequence from the beginning
    /// </summary>
    public void StartSequence()
    {
        currentTimelineIndex = 0;
        PlayTimeline(0);
    }

    /// <summary>
    /// Stop the current timeline and reset the sequence
    /// </summary>
    public void StopSequence()
    {
        if (currentTimelineIndex < timelineSequence.Count && timelineSequence[currentTimelineIndex] != null)
        {
            timelineSequence[currentTimelineIndex].Stop();
        }
        currentTimelineIndex = 0;
    }

    /// <summary>
    /// Override this method or use UnityEvents to handle sequence completion
    /// </summary>
    protected virtual void OnSequenceComplete()
    {
        // Override in derived class or add your custom logic here
    }
}
