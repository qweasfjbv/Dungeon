using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomOnMouseOver : MonoBehaviour
{
    public float duration = 0.4f; // ���̵� ��/�ƿ��� �Ϸ�Ǵµ� �ɸ��� �ð�
    private SpriteRenderer spriteRenderer;
    private Coroutine currentCoroutine = null;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(0.8f, 0, 0, 0);
    }

    void OnMouseEnter()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(FadeInAndOut(true));
    }

    void OnMouseExit()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(FadeInAndOut(false));
    }

    IEnumerator FadeInAndOut(bool loop)
    {
        while (loop)
        {
            // Fade out
            yield return FadeTo(0.0f, duration);
            // Fade in
            yield return FadeTo(0.4f, duration);
        }
        // ���콺�� ������Ʈ���� ����� ���� ���·� �����մϴ�.
        yield return FadeTo(0f, duration);
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float currentTime = 0.0f;
        float startAlpha = spriteRenderer.color.a;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, currentTime / duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            yield return null;
        }
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, targetAlpha);
    }
}
