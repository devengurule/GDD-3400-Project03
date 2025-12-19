using System;
using UnityEngine;


/// <summary>
/// Inherits from the simpler Timer, ComplexTimer holds additional information and provides reference to self on completion for easy management such as removing object
/// </summary>
public class ComplexTimer : Timer
{

    #region Fields
    Action<Timer> finishedComplexEvent = null;
    #endregion

    #region Properties 
    // Timer duration 
    #endregion

    #region Methods
    /// <summary>
    /// Adds a timer finished listener to the timer finished event, but returns reference to itself
    /// Overrides Timers AddTimerFinishedListener
    /// </summary>
    /// <param name="listener"></param>
    public void AddFinishedListener(Action<Timer> listener)
    {
        finishedComplexEvent += listener;
    }

    /// <summary>
    /// Update timer if it is running. run timer finished event when finished
    /// Calls method (setup by AddTimerFinishedListener) on completion providing reference to self
    /// </summary>
    override protected void UpdateTimer()
    {
        if (running)
        {
            elapsedSeconds += Time.deltaTime;
            if (elapsedSeconds >= totalSeconds)
            {
                running = false;
                if (finishedComplexEvent != null)
                {
                    finishedComplexEvent.Invoke(this);
                }
                if (finishedEvent != null)
                {
                    finishedEvent.Invoke();
                }
            }
        }
    }
    #endregion
}
