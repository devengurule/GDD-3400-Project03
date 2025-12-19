using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDTextManager : MonoBehaviour
{
    public static HUDTextManager Instance { get; private set; } // Singleton instance


    #region Public Fields

    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject dialoguePanel;

    [SerializeField] float typeSpeed = 0.02f; // Speed at which each character is typed out.
    [SerializeField] float minDisplayTime = 0.2f; // Minimum time to display a dialogue line before it can be dismissed.

    [SerializeField] bool tutorialEnabled = true;

    #endregion

    #region Private Fields

    private string firstTutorial = "tutorial1";

    private bool dialogueInProgress = false;
    private bool isTyping = false; // Flag indicating if the text is currently being typed out.
    private bool canDismiss = false; // Flag indicating if the current dialogue line can be dismissed by the user.
    private bool hasShownTutorial = false; // Flag for if a tutorial has already been shown or not
    private bool isSkipping = false;
    private string currentLine; // Stores the current line of dialogue being displayed.
    // Cooldown duration between valid inputs
    private float inputCooldown = 0.1f;
    private float lastInputTime = 0f;

    private EventManager eventManager;
    private Queue<string> dialogueQueue; // Queue holding the lines of dialogue to be displayed.
    private Dictionary<string, DialogueEntry> dialogueDatabase; // Dictionary holding dialogue entries, accessible via dialogue ID.
    private Coroutine animateDotsCoroutine; // Reference to the currently running coroutine that animates the looping dots.

    #endregion


    #region Unity Methods

    /// <summary>
    /// Awake is called when the script instance is being loaded
    /// </summary>
    private void Awake()
    {
        if (Instance == null) // Ensure only one instance exists
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances.
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    private void Start()
    {
        // Get reference to event manager
        eventManager = GameController.instance.EventManager;

        dialogueQueue = new Queue<string>(); // Initialize the dialogue queue.
        dialoguePanel.SetActive(false); // Hide the dialogue panel at the start.
        LoadDialogueJSON(); // Load dialogue data from a JSON file.



        if (!hasShownTutorial && tutorialEnabled)
        {
            StartCoroutine(DelayTutorial(firstTutorial));
        }
      
        // Subscribe to input events
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.TutorialToggle, OnTutorialToggle);
        }

    }

    public IEnumerator DelayTutorial(string tutorial)
    {
        //Debug.Log("tutorial starts");
        yield return new WaitForSecondsRealtime(0.5f);
        //Debug.Log("tutorial");
        StartDialogue(tutorial);
        hasShownTutorial = true;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        //Only check for inputs when 
        if (dialogueInProgress)
        {
            if (Time.unscaledTime - lastInputTime < inputCooldown)
                return; // Ignore input if still in cooldown window

            if (Input.anyKeyDown)
            {
                lastInputTime = Time.unscaledTime;

                if (isTyping)
                {
                    isSkipping = true; // Skip typewriter animation
                }
                else if (canDismiss)
                {
                    ContinueDialogue(); // Continue dialogue
                }
            }
        }
    }

    #endregion

    #region Class Methods

    /// <summary>
    /// Loads dialogue data from a JSON file into a dictionary for quick lookup.
    /// </summary>
    private void LoadDialogueJSON()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogues"); // Load the JSON file from the Resources folder.

        if (jsonFile != null)
        {
            DialogueDatabase database = JsonUtility.FromJson<DialogueDatabase>(jsonFile.text); // Deserialize the JSON text into a DialogueDatabase object.
            dialogueDatabase = new Dictionary<string, DialogueEntry>(); // Initialize the dialogue dictionary.
            foreach (DialogueEntry entry in database.dialogues) // Populate the dictionary using the dialogue entries from the JSON.
            {
                dialogueDatabase.Add(entry.id, entry); // Add key-value pairs
            }
        }
        else
        {
            Debug.LogError("Dialogue JSON file not found!"); // Log an error if the JSON file is not found.
        }
    }

    /// <summary>
    /// Begins a dialogue sequence based on the provided dialogue ID.
    /// </summary>
    /// <param name="dialogueID">The unique identifier for the dialogue sequence.</param>
    public void StartDialogue(string dialogueID)
    {
        if (tutorialEnabled)
        {
            //mark that we are in dialog
            dialogueInProgress = true;

            //pause time
            eventManager.DelayedPublish(EventType.PauseTime);

            if (dialogueDatabase != null && dialogueDatabase.ContainsKey(dialogueID)) // Check if the dialogue exists in the database.
            {
                DialogueEntry dialogue = dialogueDatabase[dialogueID];
                dialogueQueue.Clear(); // Clear any previous dialogue in the queue.

                foreach (string line in dialogue.lines) // Enqueue each line from the dialogue entry.
                {
                    dialogueQueue.Enqueue(line);
                }
                dialoguePanel.SetActive(true); // Activate the dialogue panel and reset the displayed text.
                dialogueText.text = ""; // Set initial text to blank
                StartCoroutine(DisplayNextLine()); // Start displaying the first line of dialogue.
            }
            else
            {
                Debug.LogError($"Dialogue ID {dialogueID} not found in database!"); // Log an error if the dialogue ID does not exist in the database.
            }
        }
    }

    /// <summary>
    /// Continues the dialogue when the user presses space.
    /// </summary>
    public void ContinueDialogue()
    {
        if (!canDismiss) return; // If the dialogue cannot be dismissed yet, do nothing.

        if (animateDotsCoroutine != null) // If the dot animation coroutine is running, stop it.
        {
            StopCoroutine(animateDotsCoroutine);
            animateDotsCoroutine = null;
        }

        if (isTyping) // If text is currently being typed out, complete it immediately.
        {
            StopAllCoroutines();
            dialogueText.text = currentLine;
            isTyping = false;
            canDismiss = true;
        }

        else if (dialogueQueue.Count > 0) // If there are more lines in the queue, display the next one.
        {
            StartCoroutine(DisplayNextLine());
        }

        else // If no more lines remain, end the dialogue.
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Coroutine to display the next dialogue line with a typewriter effect.
    /// </summary>
    private IEnumerator DisplayNextLine()
    {

        isTyping = true; // Indicate that the text is being typed and cannot yet be dismissed.
        canDismiss = false; // Make undismissable at the start
        dialogueText.text = ""; // Clear the displayed text.
        isSkipping = false; // Reset skip flag

        if (dialogueQueue.Count > 0) // If there is a line in the queue, dequeue it; otherwise, end dialogue.
        {
            currentLine = dialogueQueue.Dequeue();
        }
        else
        {
            EndDialogue();
            yield break;
        }

        foreach (char letter in currentLine.ToCharArray()) // Type out each character in the current line one by one.
        {
            dialogueText.text += letter;
            if (isSkipping)
            {
                dialogueText.text = currentLine; // Instant complete text
                break;
            }
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        isTyping = false; // Typing is complete.

        yield return new WaitForSecondsRealtime(minDisplayTime); // Wait for a minimum display time before allowing dismissal.
        canDismiss = true;

        if (dialogueQueue.Count > 0) // If there are more dialogue lines to follow, start the looping dots animation.
        {
            animateDotsCoroutine = StartCoroutine(AnimateDots());
        }
    }

    /// <summary>
    /// Coroutine to animate looping dots at the end of the current dialogue line.
    /// The cycle resets each time by starting with no dot.
    /// </summary>
    private IEnumerator AnimateDots()
    {
        string[] dotStates = new string[] { "", ".", "..", "..." }; // Define an array of dot states. The first element (empty string) ensures no dot at cycle start.

        while (dialogueQueue.Count > 0) // Continue the animation while there is more dialogue queued.
        {
            for (int i = 0; i < dotStates.Length; i++) // Iterate over each state in the dotStates array.
            {

                dialogueText.text = currentLine + dotStates[i]; // Update the dialogue text with the current line plus the dot state.

                if (i == 0) // For the empty state, use a shorter delay; for the others, use the normal delay.
                    yield return new WaitForSecondsRealtime(0.15f);
                else
                    yield return new WaitForSecondsRealtime(0.5f);
            }
        }
    }

    /// <summary>
    /// Ends the dialogue by clearing text and hiding the dialogue panel.
    /// </summary>
    private void EndDialogue()
    {
        //mark that we are in dialog
        dialogueInProgress = false;

        //reset dialogbox and hide
        dialogueText.text = "";
        dialoguePanel.SetActive(false);

        //Unpause time
        eventManager.Publish(EventType.ResumeTime);
    }

    #endregion

    #region Events
    /// <summary>
    /// When Tutorial Toggle event is raised, see if an explicit value was provided
    ///     If so then use that
    ///     Otherwise simply change the current Tutorial value
    ///    Used to disable the 
    /// </summary>
    /// <param name="toggleData">If a bool is provided then this will be used to set explicitly that the tutorial is enabled (true) or disabled (false)</param>
    public void OnTutorialToggle(object toggleData)
    {
        if (toggleData is bool value)
        {
            tutorialEnabled = value;
        }
        else
        {
            tutorialEnabled = !tutorialEnabled;
        }

        if (!tutorialEnabled)
        {
            EndDialogue();
        }
    }
    #endregion
}