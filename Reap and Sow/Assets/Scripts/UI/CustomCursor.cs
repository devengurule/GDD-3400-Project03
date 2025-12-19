using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D plantCursor;
    [SerializeField] private Texture2D harvestCursor;
    private static CustomCursor instance;

    private Vector2 hotspot;
    private Texture2D currentCursor;
    private float hoverRadius = 0.5f;

    private void Start()
    {
     
        hotspot = new Vector2(defaultCursor.width * 0.5f, defaultCursor.height * 0.5f);
        SetCursor(defaultCursor);
    }

    private void Update()
    {
        HoverCheck();
    }

    private void HoverCheck()
    {
        Vector2 screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapCircleAll(screenPos, hoverRadius);

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                GameObject target = hit.gameObject;
                //Debug.Log("Hit: " + target.name + " | Tag: " + target.tag);

                if (target.CompareTag("EnemyTag"))
                {
                    SetCursor(attackCursor);
                    return;
                }
                if (target.CompareTag("PlotTag"))
                {
                    SetCursor(plantCursor);
                    return;
                }
                //if (target.CompareTag("Harvest"))
                //{
                //    SetCursor(harvestCursor);
                //    return;
                //}
            }
        }

        SetCursor(defaultCursor);
    }

    private void SetCursor(Texture2D cursorTex)
    {
        //if (currentCursor == cursorTex) return;
        //currentCursor = cursorTex;
        hotspot = new Vector2(defaultCursor.width * 0.5f, defaultCursor.height * 0.5f);
        Cursor.SetCursor(cursorTex, hotspot, CursorMode.Auto);
    }
}