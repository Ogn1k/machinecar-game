using UnityEngine;

public class BoostTrigger : MonoBehaviour
{
    public float boostForce;
    public bool forward;
    public bool gravity = true;
    public Vector3 direction;
    void OnTriggerEnter(Collider other)
    {
        Rigidbody car = other.GetComponent<Rigidbody>();
        if (car != null)
        {
            if(forward)
                car.AddForce(car.transform.forward * boostForce, ForceMode.Impulse);
            else
                car.AddForce(direction * boostForce, ForceMode.Impulse);
            if(!gravity)
                other.gameObject.GetComponent<Rigidbody>().useGravity = false;
        }
    }
}
