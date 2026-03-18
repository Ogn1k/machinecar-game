using UnityEngine;
using TMPro;
using System;

public class text : MonoBehaviour
{
    public float duration = 2f;
    public bool fadeInFromLeft = true; // true: появляется слева направо, false: справа налево
    public float fadeRange = 20f; // область плавности в пикселях
    private TMP_Text textMesh;
    private float startTime;
    private bool isAnimating = false;

    public event Action OnAnimationEnd;

    public void AnimationEnded()
    {
        gameObject.GetComponent<Animator>().SetTrigger("animEnd");
        OnAnimationEnd?.Invoke();
    }

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    public void Play()
    {
        startTime = Time.time;
        isAnimating = true;
    }

    void Update()
    {
        if (!isAnimating) return;

        float t = (Time.time - startTime) / duration;
        t = Mathf.Clamp01(t);

        ApplyFade(t);

        if (t >= 1f)
        {
            isAnimating = false;
        }
    }

    void ApplyFade(float progress)
    {
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        // Находим глобальные minX и maxX среди всех символов
        float globalMinX = float.MaxValue;
        float globalMaxX = float.MinValue;

        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                float x = vertices[vertexIndex + j].x;
                if (x < globalMinX) globalMinX = x;
                if (x > globalMaxX) globalMaxX = x;
            }
        }

        // Порог в зависимости от направления
        float threshold;
        if (fadeInFromLeft)
        {
            threshold = Mathf.Lerp(globalMinX, globalMaxX, progress);
        }
        else
        {
            threshold = Mathf.Lerp(globalMaxX, globalMinX, progress);
        }

        // Применяем эффект к каждому символу
        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Вычисляем характерную точку символа для сравнения с порогом
            float charPos;
            if (fadeInFromLeft)
            {
                // Для появления слева направо используем правую границу
                charPos = GetRightX(vertices, vertexIndex);
            }
            else
            {
                // Для появления справа налево используем левую границу
                charPos = GetLeftX(vertices, vertexIndex);
            }

            float alphaFactor = 1f;
            if (fadeInFromLeft)
            {
                if (charPos > threshold)
                {
                    float distance = charPos - threshold;
                    alphaFactor = Mathf.Clamp01(1f - (distance / fadeRange));
                }
            }
            else
            {
                if (charPos < threshold)
                {
                    float distance = threshold - charPos;
                    alphaFactor = Mathf.Clamp01(1f - (distance / fadeRange));
                }
            }

            byte alpha = (byte)(alphaFactor * 255);

            for (int j = 0; j < 4; j++)
            {
                Color32 c = colors[vertexIndex + j];
                c.a = alpha;
                colors[vertexIndex + j] = c;
            }
        }

        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private float GetRightX(Vector3[] vertices, int startIndex)
    {
        float maxX = float.MinValue;
        for (int i = 0; i < 4; i++)
        {
            if (vertices[startIndex + i].x > maxX)
                maxX = vertices[startIndex + i].x;
        }
        return maxX;
    }

    private float GetLeftX(Vector3[] vertices, int startIndex)
    {
        float minX = float.MaxValue;
        for (int i = 0; i < 4; i++)
        {
            if (vertices[startIndex + i].x < minX)
                minX = vertices[startIndex + i].x;
        }
        return minX;
    }
}