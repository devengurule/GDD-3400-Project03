using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Rather than using the standard Event system this Event Manager is Delegate based.
///     This allows us to simply use a delegate dictionary, rather than having to update eventManager with every new Event Type as it is created.
/// Relies on EventType enumeration to differentiate dictionary entries
/// 
/// </summary>
public class EventManager : MonoBehaviour
{
    //Dictionary to hold events and their associated listeners
    /*/ String used to reference events by type
        Action<> to hold the delegated event method
            object to pass eventargs or other through the event
    /*/
    [SerializeField] private Dictionary<EventType, Action<object>> eventDictionary;

    void Awake()
    { 
        //Make object persistent between rooms
        DontDestroyOnLoad(gameObject);

        //Initialize Dictionary
        eventDictionary = new Dictionary<EventType, Action<object>>();
    }

    /// <summary>
    /// Subscribe method allows an object to add a method to be called any time a specific event type is raised
    /// </summary>
    /// <param name="eventType">Type of event to subscribe to</param>
    /// <param name="listener">Delegate Method to call when the event is raised</param>
    public void Subscribe(EventType eventType, Action<object> listener)
    {
        if (eventDictionary != null)
        {
            //Check if such an event exists in the dictionary yet
            if (!eventDictionary.ContainsKey(eventType))
            {
                // Initialize the event entry if it doesn't exist
                eventDictionary[eventType] = delegate { };
            }

            // Add listener to the event
            eventDictionary[eventType] += listener;
        }
    }

    /// <summary>
    /// Subscribe method allows an object to add a method to be called any time a specific event type is raised
    /// </summary>
    /// <param name="eventType">Type of event to unsubscribe from</param>
    /// <param name="listener">Delegate Method call to remove from dictionary</param>
    public void Unsubscribe(EventType eventType, Action<object> listener)
    {
        if (eventDictionary != null && eventDictionary.ContainsKey(eventType))
        {
            // Remove listener from the event
            eventDictionary[eventType] -= listener;

            // Clean up if no listeners remain
            if (eventDictionary[eventType] == null)
            {
                eventDictionary.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// Method for publishing an event to notify all listeners
    /// </summary>
    /// <param name="eventType">Type of event to raise</param>
    /// <param name="args">An any needed object, defaults to Null</param>
    public void Publish(EventType eventType, object value = null)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            // Invoke all listeners associated with the event
            //      On error publish debug message rather than crashing
            try
            {
                eventDictionary[eventType]?.Invoke(value);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error invoking event {eventType}: {e.Message} {value}");
            }
        }
    }

    /// <summary>
    /// DelayedPublish is the same as publish but will delay for <paramref name="delay"/> seconds.
    ///     This uses a Coroutine so that other scripts can continue running, then publishes a short while after.
    ///     Main uisage for this is for events sent just before destroying the publishing object. 
    ///     0.5f should be enough delay that the object finishes destroying itself first.
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="value"></param>
    /// <param name="delay"></param>
    public void DelayedPublish(EventType eventType, object value = null, float delay = 0.05f)
    {
        StartCoroutine(DelayedPublishCoroutine(eventType, value, delay));
    }

   private IEnumerator DelayedPublishCoroutine(EventType eventType, object value, float delay)
    {
        yield return new WaitForSeconds(delay);
        Publish(eventType, value);
    }

    /// <summary>
    /// Tells the eventManager to remove ALL subscribers
    /// </summary>
    public void Reset()
    {
        eventDictionary.Clear();
    }
}