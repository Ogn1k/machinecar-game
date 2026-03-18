using Unity.Mathematics;
using UnityEngine;

public class SectionTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        CarController3 car = other.GetComponent<CarController3>();
        if (car == null) return;

        RaycastHit hit;
        if (Physics.Raycast(car.transform.position, -car.playerRB.linearVelocity.normalized, out hit, 6f))
        {
            car.inLoop = true;
            car.loopNormal = hit.normal;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CarController3 car = other.GetComponent<CarController3>();
        if (car == null) return;

        car.inLoop = false;
    }
}
