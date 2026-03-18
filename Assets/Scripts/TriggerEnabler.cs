using UnityEngine;

public class TriggerEnabler : MonoBehaviour
{
    public GameObject obj;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            obj.SetActive(true);
        }
    }
}
