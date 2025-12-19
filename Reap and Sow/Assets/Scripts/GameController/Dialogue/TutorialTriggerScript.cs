using System.Collections.Generic;
using UnityEngine;

public class TutorialTriggerScript : MonoBehaviour
{
    [SerializeField]
    private string dialogueID = "tutorial1"; // Dialogue ID (modifiable in Inspector)

    private GameController gameController; // Reference to GameController

    /// <summary>
    /// Called when another collider enters the trigger collider attached to this GameObject.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerTag")) // Check if the Player entered
        {
            // Ensure the key exists and has not been triggered before
            if (gameController.tutorialDict.ContainsKey(dialogueID) && !gameController.tutorialDict[dialogueID])
            {
                gameController.tutorialDict[dialogueID] = true; // Mark as triggered
                GetComponent<BoxCollider2D>().enabled = false;

                // Start dialogue using HUDTextManager
                HUDTextManager.Instance.StartDialogue(dialogueID);
            }

            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Assign GameController instance
        gameController = GameController.instance;
        if (gameController == null)
        {
            Debug.LogError("GameController instance not found!");
        }

        // Ensure the dictionary is initialized
        if (gameController.tutorialDict == null)
        {
            gameController.tutorialDict = new Dictionary<string, bool>();
        }

        // Add the tutorial entry if it does not exist
        if (!gameController.tutorialDict.ContainsKey(dialogueID))
        {
            gameController.tutorialDict.Add(dialogueID, false);
        }
    }

    private void OnDrawGizmos()
    {
        // Visual representation in Unity Editor
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.DrawCube(transform.position, boxCollider.size);
        }
    }
}