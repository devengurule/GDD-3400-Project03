using UnityEngine;

public class AimCursorScript : MonoBehaviour
{
    [SerializeField] private Sprite defaultCursor;
    [SerializeField] private Texture2D defaultCursorTexture;
    [SerializeField] private float cursorSize;
    private SpriteRenderer sr;
    private Transform cursorTransform;
    private EventManager eventManager;
    private float initialZPos;

    void Start()
    {
        eventManager = GameController.instance.EventManager;
        Cursor.visible = false;
        sr = GetComponent<SpriteRenderer>();
        cursorTransform = GetComponent<Transform>();
        initialZPos = cursorTransform.parent.position.z;

        // Set cursor sprite to default cursor
        sr.sprite = defaultCursor;

        Cursor.SetCursor(defaultCursorTexture, new Vector2(defaultCursorTexture.width/2, defaultCursorTexture.height / 2), CursorMode.Auto);

        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.Pause, OnPause);
        }
    }
    private void OnDestroy()
    {
        eventManager.Unsubscribe(EventType.Pause, OnPause);
    }
    void Update()
    {
        cursorTransform.localScale = new Vector3(1f, 1f, 1f) * cursorSize;

        // Assign Object position to mouse position in world coordinates
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = initialZPos;
        cursorTransform.position = mouseWorldPosition;
    }

    void OnPause(object target)
    {         
        if (sr.enabled)
        {
            sr.enabled = false;
            Cursor.visible = true;
        }
        else
        {
            sr.enabled = true;
            Cursor.visible = false;
        }
    }
}
