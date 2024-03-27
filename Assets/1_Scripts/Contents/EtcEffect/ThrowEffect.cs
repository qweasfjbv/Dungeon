using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ThrowEffect : MonoBehaviour
{
    private float endScale = 0.5f;

    public void Throw(Vector3 startPos, Vector3 endPos, float throwTime)
    {
        //StartCoroutine(ThrowCoroutine(startPos, endPos, throwTime));
        this.transform.position = startPos;
        transform.DOMove(endPos, throwTime).SetEase(Ease.OutCirc).onComplete = ThrowDone;
        transform.DORotate(new Vector3(0, 0, 360), throwTime/2, RotateMode.FastBeyond360).SetLoops(2, LoopType.Incremental);
        transform.DOScale(new Vector3(0.5f, 0.5f, 1), throwTime).SetEase(Ease.OutCubic);

    }

    public void ThrowDone()
    {
        Destroy(this.gameObject);
    }
    private IEnumerator ThrowCoroutine(Vector3 startPos, Vector3 endPos, float throwTime)
    {
        float elapsedTime = 0;

        // 초기 위치와 크기 설정
        transform.position = startPos;
        float startScale = transform.localScale.x;
        float endScale =1; 

        while (elapsedTime < throwTime)
        {
            elapsedTime += Time.deltaTime;

            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / throwTime);
            float scale = Mathf.Lerp(startScale, endScale, elapsedTime / throwTime);
            transform.localScale = new Vector3(scale, scale, endScale);
            transform.Rotate(0, 0, Time.deltaTime * 720f);

            yield return null;
        }

        transform.position = endPos;
        transform.localScale = new Vector3(endScale, endScale, 1);


        Destroy(gameObject);
    }
}
