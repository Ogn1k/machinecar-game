using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject menu;
    public Transform player;
    bool flag = false;
    float delay1;
    bool delay1flag = true;
    public TMP_Text stuck;
    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    void Update()
    {
        if(menu && Input.GetKeyDown(KeyCode.Escape))
        {
            menu.SetActive(flag);
            flag=!flag;
        }
        if(!delay1flag && stuck)
        {
            delay1 -= Time.deltaTime;
            stuck.text = ((int)delay1).ToString();
            if(delay1<0)
            {
                delay1=10;
                delay1flag=true;
                stuck.text = "Застрял";
            }
        }
    }

    public void Unstuck()
    {
        if(player && delay1flag)
        {
            delay1flag = false;
            if(player.position.z>=0)
                player.position = new Vector3(player.position.x, player.position.y+5, player.position.z-5); 

            if(player.position.z<0)
                player.position = new Vector3(player.position.x, player.position.y+5, player.position.z+5); 
        }
    }
}
