using UnityEngine;

public class RoadChunk : MonoBehaviour
{
    public GameObject startpoint;
    public GameObject endpoint;
    public bool animeQuiz;

    // вызываться, когда чанк "активируется"
    public void OnSpawn()
    {
        gameObject.SetActive(true);
        if(animeQuiz)
            gameObject.GetComponent<AnimeQuiz>().OnSpawn();
    }

    // когда чанк больше не нужен
    public void OnDespawn()
    {
        if(animeQuiz)
            gameObject.GetComponent<AnimeQuiz>().OnDespawn();
        gameObject.SetActive(false);
    }
}
