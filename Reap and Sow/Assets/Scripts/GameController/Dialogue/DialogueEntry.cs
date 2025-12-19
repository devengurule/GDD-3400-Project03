using System.Collections.Generic;

/// <summary>
/// Represents a single dialogue entry with a unique identifier and its associated lines.
/// </summary>
[System.Serializable]
public class DialogueEntry
{
    public string id; // The unique identifier for this dialogue entry.
    public List<string> lines; // The list of dialogue lines associated with this entry.
}

/// <summary>
/// Contains a collection of dialogue entries to form a dialogue database.
/// </summary>
[System.Serializable]
public class DialogueDatabase
{
    public List<DialogueEntry> dialogues; // The list of dialogue entries within the database.
}
