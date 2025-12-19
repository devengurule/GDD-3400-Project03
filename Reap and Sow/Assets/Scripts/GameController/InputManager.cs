using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Store event manager for easy publishing
    private EventManager eventManager;
    InputBinding inputActions;

    void Start()
    {
        eventManager = GameController.instance.EventManager;

        if (eventManager == null)
        {
            Debug.LogError("EventManager not found in GameController.");
        }

    }

    #region Player Event Handlers
    // Convert Player Inputs into eventManager events
    private void OnMove(InputValue value)
    {
        Vector2 vector = value.Get<Vector2>().normalized;
        eventManager.Publish(EventType.Move,vector);
    }
    private void OnAttack()
    {
        eventManager.Publish(EventType.Attack, Input.mousePosition);
    }
    private void OnFire()
    {
        eventManager.Publish(EventType.Fire, Input.mousePosition);
    }
    private void OnPrevious()
    {
        eventManager.Publish(EventType.PrevItem);
    }
    private void OnNext()
    {
        eventManager.Publish(EventType.NextItem);
    }

    private void OnPlant()
    {
        eventManager.Publish(EventType.Plant);
    }
    private void OnPause()
    {
        eventManager.Publish(EventType.Pause);
    }
    #endregion
}