using System.IO;
using UnityEngine;

public class ClearSaves : MonoBehaviour
{
    private string savePath;
    private EventManager eventManager;

    private void Start()
    {
        savePath = Application.persistentDataPath + "/save.json";
        eventManager = GameController.instance.EventManager;
        if (eventManager != null)
        {
            Debug.Log("EventManager found — subscribing SaveGame event.");
            eventManager.Subscribe(EventType.ClearSave, ClearSaveFile);
        }
        else
        {
            Debug.LogWarning("EventManager not found!");
        }
    }
    public void ClearSaveFile(object target)
    {
        ClearSave();
    }

    public void ClearSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save file deleted successfully!");
        }
        else
        {
            Debug.LogWarning("No save file found to delete.");
        }
    }
}
