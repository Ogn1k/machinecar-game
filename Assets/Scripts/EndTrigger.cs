using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    public GameObject sectionBlockRear;
    public BoostTrigger booster;
    UIController uiCon;
    public List<GameObject> roads;
    int dir;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            uiCon = other.GetComponent<UIController>();
            uiCon.finalTMP.GetComponent<text>().OnAnimationEnd += HandleAnimationEnd;
            dir = uiCon.Satan();
            sectionBlockRear.SetActive(true);

        }
    }

    void HandleAnimationEnd()
    {
        //print("handled");
        StartCoroutine(HandleAnimationEndCoroutine());
    }

    IEnumerator HandleAnimationEndCoroutine()
    {
        yield return new WaitForSeconds(4f);
        booster.direction = new Vector3(0, dir, 0);
        booster.gameObject.SetActive(true);
        if(dir==-1)
        {
            booster.boostForce = 60000;
            foreach(var road in roads)
                road.SetActive(false);
        }
    }
}
