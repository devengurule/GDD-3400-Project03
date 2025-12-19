// InputSimulator.cs
using UnityEngine;

/// <summary>
/// Simulates player input by sending movement/attack/fire/slot-change
/// through the EventManager (same channel as real input).
/// </summary>
public class InputSimulator
{
    /// <summary> Simulate WASD (normalized) movement. </summary>
    public static void Move(Vector2 direction)
    {
        if (direction.sqrMagnitude > 1f) direction.Normalize();
        GameController.instance.EventManager.Publish(EventType.Move, direction);
    }

    /// <summary> Simulate a attack at a screen-space position. </summary>
    public static void Attack(Vector3 screenPos)
    {
        // Matches InputManager.OnAttack => EventType.Attack, mouse position
        GameController.instance.EventManager.Publish(EventType.Attack, screenPos);
    }

    /// <summary> Simulate a ranged �Fire� at a screen-space position. </summary>
    public static void Fire(Vector3 screenPos)
    {
        // Matches InputManager.OnFire => EventType.Fire, mouse position
        GameController.instance.EventManager.Publish(EventType.Fire, screenPos);
    }

    /// <summary> Cycle to next inventory slot. </summary>
    public static void NextItem()
    {
        GameController.instance.EventManager.Publish(EventType.NextItem);
    }

    /// <summary> Cycle to previous inventory slot. </summary>
    public static void PrevItem()
    {
        GameController.instance.EventManager.Publish(EventType.PrevItem);
    }
    /// <summary> Cycle to previous inventory slot. </summary>
    public static void Pause()
    {
        GameController.instance.EventManager.Publish(EventType.Pause);
    }

    /// <summary> Stop all movement. </summary>
    public static void StopMove()
    {
        Move(Vector2.zero);
    }
}
