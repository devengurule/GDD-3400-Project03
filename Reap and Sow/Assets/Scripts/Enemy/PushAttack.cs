using UnityEngine;

public class PushAttack : MonoBehaviour
{

    // Values for push damage and push force
    [SerializeField] int pushDamage = 1;
    [SerializeField] float pushForce = 10f;

    // I don't like the idea of creating a script that just holds these two values  and
    // their getters/setters but it'll do

    public int GetPushDamage() { return pushDamage; }

    public void SetPushDamage(int pushDamage) { this.pushDamage = pushDamage; }

    public float GetPushForce() { return pushForce; }

    public void SetPushForce(float pushForce) { this.pushForce = pushForce; }
    
}
