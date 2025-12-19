using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        ConfigurationUtils.Initialize();
    }
}
