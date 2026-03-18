using System.Collections.Generic;
using UnityEngine;

public class QuizController : MonoBehaviour
{
    public AudioSource playerAS;
    public List<QuizQuestion> quizQuestions;
    public List<Sprite> quizImagesAll;

    [HideInInspector] public List<AnimeQuiz> activeQuizzes = new List<AnimeQuiz>();

    void Update()
    {
        // Очищаем мёртвые чанки в начале списка (на случай, если RemoveQuiz не сработал)
        while (activeQuizzes.Count > 0 && 
              (activeQuizzes[0] == null || !activeQuizzes[0].gameObject.activeInHierarchy))
        {
            Debug.Log($"Removing dead quiz: {activeQuizzes[0]?.name}");
            activeQuizzes.RemoveAt(0);
        }

        // Если музыка не играет и есть чанки – запускаем первый
        if (!playerAS.isPlaying && activeQuizzes.Count > 0)
        {
            PlayMusic(activeQuizzes[0]);
        }
    }

    void PlayMusic(AnimeQuiz quiz)
    {
        if (quiz == null) return;
        var clip = quizQuestions[quiz.randomQuiz].audioClip;
        if (clip == null)
        {
            Debug.LogWarning($"Quiz {quiz.name} has no audio clip!");
            return;
        }
        playerAS.Stop(); // останавливаем текущую музыку
        playerAS.PlayOneShot(clip);
        Debug.Log($"Started playing music for {quiz.name}");
    }

    public void StopMusic()
    {
        playerAS.Stop();
    }

    public void AddQuiz(AnimeQuiz quiz)
    {
        bool wasEmpty = activeQuizzes.Count == 0;
        activeQuizzes.Add(quiz);
        Debug.Log($"Added quiz {quiz.name}. Total: {activeQuizzes.Count}");

        if (wasEmpty)
        {
            // Если очередь была пуста, но музыка играет (сиротский трек) – останавливаем
            if (playerAS.isPlaying)
            {
                playerAS.Stop();
            }
            // Запускаем музыку нового чанка (он теперь первый)
            PlayMusic(quiz);
        }
    }

    public void RemoveQuiz(AnimeQuiz quiz)
    {
        int index = activeQuizzes.IndexOf(quiz);
        if (index == -1) return;

        bool wasFirst = (index == 0);
        activeQuizzes.RemoveAt(index);
        Debug.Log($"Removed quiz {quiz.name}. Was first: {wasFirst}. Remaining: {activeQuizzes.Count}");

        // Если удалённый был первым и играл – останавливаем музыку и запускаем следующий (если есть)
        if (wasFirst && activeQuizzes.Count>=1)
        {
            playerAS.Stop(); // важно: останавливаем, чтобы следующий запустился в Update
            // Следующий запустится в ближайшем Update, т.к. !playerAS.isPlaying станет true
        }
    }

    public void CompleteQuiz(AnimeQuiz quiz)
    {
        // При прохождении делаем то же самое, что и при удалении
        RemoveQuiz(quiz);
    }
}
[System.Serializable]
public class QuizQuestion
{
    public Sprite image;
    public AudioClip audioClip;
}