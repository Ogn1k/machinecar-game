using UnityEngine;

public class QuizTrigger : MonoBehaviour
{
    public AnimeQuiz quiz;
    public bool right;

    public void OnSpawn()
    {
        gameObject.SetActive(true);
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            UIController uiController = other.GetComponent<UIController>();
            if(right)
            {
                uiController.AddKarma();
            }
            else
            {
                uiController.SubtractKarma();
            }
            if (quiz != null)
                quiz.PopQuiz();
            else
                Debug.LogError("QuizTrigger: quiz is null!");
                
            gameObject.SetActive(false);
        }
    }
}
