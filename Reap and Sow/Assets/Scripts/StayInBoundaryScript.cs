using UnityEngine;

public class StayInBoundaryScript : MonoBehaviour
{
    private float minX, maxX, minY, maxY;

    void Start()
    {
        // Setting screen boundaries
        Camera mainCamera = Camera.main;
        Vector3 screenBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 screenTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        minX = screenBottomLeft.x;
        maxX = screenTopRight.x;
        minY = screenBottomLeft.y;
        maxY = screenTopRight.y;
    }

    void Update()
    {
        Vector2 position = transform.position;

        // Clamp the position within the screen boundaries
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        transform.position = position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("BarrierTag"))
        {
            // Handle collision with barrier
            Vector2 position = transform.position;

            // Change position to stay within boundaries
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);

            transform.position = position;
        }
    }
}
