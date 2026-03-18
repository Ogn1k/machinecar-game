using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TMP_Text spdText;
    public TMP_Text karmaText;
    public Transform needle;
    public Image speedometer;
    public float minNeedleRotation;
    public float maxNeedleRotation;
    public RectTransform mark;
    public float minMarkXPos;
    public float maxMarkXPos;
    public float endOfRoadX;
    public GameObject Bay;
    public TMP_Text finalTMP;
    Animator textAnim;
    float speed;
    public float redLine;
    int karma;
    Color orange;
    

    void Start()
    {
        textAnim = finalTMP.gameObject.GetComponent<Animator>(); 
        karma = 0;
        ColorUtility.TryParseHtmlString( "#FF6F00" , out orange );
    }

    void Update()
    {
        SetSpeed();
        SetNeedle();
        SetRoadProgress();
    }

    public void SetSpeed()
    {
        speed = gameObject.GetComponent<CarController3>().GetSpeed();
        spdText.text = "Скорость:" + speed.ToString();
        
    }

    void SetNeedle()
    {

        needle.rotation = Quaternion.Euler(0,0, Mathf.Lerp(minNeedleRotation, maxNeedleRotation, speed/redLine));
        if(speed>redLine)
        {
            speedometer.color = orange;
            Bay.SetActive(true);
        }
        else
        {
            speedometer.color = Color.white;
            Bay.SetActive(false);
        }
    }

    void SetRoadProgress()
    {
        mark.localPosition = new Vector3(Mathf.Lerp(minMarkXPos, maxMarkXPos, transform.position.x/endOfRoadX),0,0);
    }

    void SetKarma()
    {
        karmaText.text = "Карма: " + karma.ToString();
    }

    public void AddKarma()
    {
        karma++;
        SetKarma();
    }

    public void SubtractKarma()
    {
        karma--;
        SetKarma();
    }

    public int Satan()
    {
        
        finalTMP.gameObject.SetActive(true);
        if(karma>0)
        {
            finalTMP.text = "твоя карма слишком высока,\n пора в ад, анимешник";
            textAnim.SetTrigger("slideDown");
            return -1;
        }
        if(karma<0)
        {
            finalTMP.text = "твоя карма низка, мужик,\n иди с миром";
            textAnim.SetTrigger("fadeIn");
            finalTMP.gameObject.GetComponent<text>().Play();
            return 1;
        }
        if(karma==0)
        {
            finalTMP.text = "gmmmmmm asdasfqdsaf что с кармой\n хз что делать ";
            textAnim.SetTrigger("rotate");
            return 0;
        }
        return 0;
    }

    
}
