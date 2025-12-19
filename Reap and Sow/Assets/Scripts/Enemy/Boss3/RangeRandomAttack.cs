using UnityEngine;
using TargetUtils;

public class RangeRandomAttack : RangeAttack
{
    #region Serialized Fields
    [SerializeField] private bool useRandomTarget;
    [SerializeField] private Vector2 targetBox;
    [SerializeField] private Vector2 boxOffset;
    #endregion

    #region Fields
    private GameObject randomTarget;
    private Vector3 targetBoxPos;
    private Vector2 randomTargetPos;
    private float targetDist;
    #endregion

    protected override void Reload(float delay)
    {
        if (useRandomTarget)
        {
            randomFire = true;
            targetBoxPos = new Vector3(boxOffset.x, boxOffset.y, transform.position.z);

            // Get random target inside target box
            float x = Random.Range((targetBoxPos.x) - (targetBox.x / 2), (targetBoxPos.x) + (targetBox.x / 2));
            float y = Random.Range((targetBoxPos.y) - (targetBox.y / 2), (targetBoxPos.y) + (targetBox.y / 2));
            randomTargetPos = new Vector2(x, y);

            //get the randomTargetPos as a transform (so can use existing Fire() logic
            if (randomTarget == null) randomTarget = new GameObject("RandomTarget");
            randomTarget.transform.position = randomTargetPos;
            targetTransform = randomTarget.transform;
        }

        base.Reload(delay);
    }

    private void OnDrawGizmos()
    {
        Color green = new Color(0, 255, 0, 0.05f);
        Color red = new Color(255, 0, 0, 1f);
        Color blue = new Color(0, 0, 255, 1f);

        Gizmos.color = green;
        Gizmos.DrawWireSphere(transform.position, maxRange);
        Gizmos.color = red;
        Gizmos.DrawWireSphere(transform.position, targetDist);
        Gizmos.color = blue;
        Gizmos.DrawWireSphere(randomTargetPos, 0.1f);

        if (useRandomTarget)
        {
            Vector3 pos = new Vector3(boxOffset.x, boxOffset.y, transform.position.z);

            Gizmos.color = red;

            Gizmos.DrawWireCube(pos, targetBox);
            Gizmos.color = blue;
        }
    }
}