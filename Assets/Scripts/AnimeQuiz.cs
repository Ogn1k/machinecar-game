using UnityEngine;
using UnityEngine.UI;

public class AnimeQuiz : MonoBehaviour
{
    public Image quizImage1;
    public Image quizImage2;
    public QuizController quizController; // можно оставить пустым, будет найден в OnSpawn
    public QuizTrigger quizTrigger1;
    public QuizTrigger quizTrigger2;
    public int randomQuiz;

    Image[] quizImages;
    QuizTrigger[] quizTriggers;

    public void OnSpawn()
    {
        if (quizController == null)
            quizController = GameObject.Find("QuizController")?.GetComponent<QuizController>();
        if (quizController == null)
            Debug.LogError("QuizController not found!");

        InitQuiz();
        quizController.AddQuiz(this);
        foreach (var trigger in quizTriggers)
            trigger.OnSpawn();
    }

    public void OnDespawn()
    {
        if (quizController != null)
            quizController.RemoveQuiz(this);
    }

    void InitQuiz()
    {
        quizImages = new Image[2] { quizImage1, quizImage2 };
        quizTriggers = new QuizTrigger[2] { quizTrigger1, quizTrigger2 };
        randomQuiz = Random.Range(0, quizController.quizQuestions.Count);
        int randomImage = Random.Range(0, quizController.quizImagesAll.Count);
        int randomImagePick = Random.Range(0, 2);

        quizImages[randomImagePick].sprite = quizController.quizQuestions[randomQuiz].image;
        quizImages[1 - randomImagePick].sprite = quizController.quizImagesAll[randomImage];

        quizTriggers[randomImagePick].right = true;
        quizTriggers[1 - randomImagePick].right = false;

        foreach (QuizTrigger trigger in quizTriggers)
            trigger.quiz = this;
    }

    public void PopQuiz()
    {
        quizController.CompleteQuiz(this);
    }
}