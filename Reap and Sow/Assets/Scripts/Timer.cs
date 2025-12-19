using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// A basic class for time based events 
/// </summary>
public class Timer : MonoBehaviour
{
    #region Fields
    // Timer duration 
    protected float totalSeconds;
    [SerializeField]
    string timerName;

    // Timer execution
    [SerializeField]
    protected float elapsedSeconds;
    protected bool running;

    // Support for the finished property
    bool started;

    protected TimerFinishedEvent finishedEvent = new TimerFinishedEvent();
    #endregion

    #region Properties 
    public string TimerName { get => timerName; set => timerName = value; }

    /// <summary>
    /// Gets total time of timer
    /// Sets total time if the timer is not currently running
    /// </summary>
    public float Duration
    {
        get => totalSeconds;
        set
        {
            if (!running)
            {
                totalSeconds = value;
            }
        }
    }

    /// <summary>
    /// Is this timer finished running??
    /// </summary>
    public bool Finished
    {
        get { return started && !running; }
    }

    /// <summary>
    /// Is this timer still active
    /// </summary>
    public bool Running
    {
        get { return running; }
    }

    /// <summary>
    /// How much time is left 
    /// </summary>
    public float SecondsLeft
    {
        get
        {
            if (running)
            {
                return totalSeconds - elapsedSeconds;
            }
            else
            {
                return 0;
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Stops the timer
    /// </summary>
    public void Stop()
    {
        started = false;
        running = false;
        elapsedSeconds = 0;
    }


    /// <summary>
    /// Adds time to the timer
    /// </summary>
    /// <param name="time"></param>
    public void AddTime(float time)
    {
        totalSeconds += time;
    }

    /// <summary>
    /// Adds a timer finished listener to the timer finished event
    /// </summary>
    /// <param name="listener"></param>
    public void AddTimerFinishedListener(UnityAction listener)
    {
        finishedEvent.AddListener(listener);
    }
    public void ClearTimerFinishedListener()
    {
        finishedEvent.RemoveAllListeners();
    }

    /// <summary>
    /// Update timer if it is running. run timer finished event when finished
    /// </summary>
    protected virtual void Update()
    {
        UpdateTimer();
    }

    /// <summary>
    /// add time, then check for completion. On completion call any listeners
    /// </summary>
    protected virtual void UpdateTimer()
    {

        if (running)
        {
            elapsedSeconds += Time.deltaTime;
            if (elapsedSeconds >= totalSeconds)
            {
                running = false;
                Reset();
                if (finishedEvent != null)
                {
                    finishedEvent.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// Starts the timer
    /// </summary>
    public void Run()
    {
        if (totalSeconds > 0)
        {
            started = true;
            running = true;
            elapsedSeconds = 0;
        }
        else
        {
            started = false;
            running = false;
            elapsedSeconds = 0;
            if (finishedEvent != null)
            {
                finishedEvent.Invoke();
            }
        }
    }

    /// <summary>
    /// Reset the timer
    /// </summary>
    public void Reset()
    {
        Stop();
        elapsedSeconds = 0;
        started = false;
    }

    /// <summary>
    /// Pause the Timer (without resetting)
    /// </summary>
    public void Pause() => running = false;

    /// <summary>
    /// Resume paused timer (if paused)
    /// </summary>
    public void Resume()
    {
        if (started && !running)
            running = true;
    }

    #endregion

}
