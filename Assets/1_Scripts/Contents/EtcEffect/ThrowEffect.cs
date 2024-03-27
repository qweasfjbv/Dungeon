using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ThrowEffect : MonoBehaviour
{
    private float endScale = 0.5f;

    public void Throw(Vector3 startPos, Vector3 endPos, float throwTime)
    {
        this.transform.position = startPos;
        transform.DOMove(endPos, throwTime).SetEase(Ease.OutCirc).onComplete = ThrowDone;
        transform.DORotate(new Vector3(0, 0, 360), throwTime/3, RotateMode.FastBeyond360).SetLoops(3, LoopType.Incremental).SetEase(Ease.Linear);
        transform.DOScale(new Vector3(0.5f, 0.5f, 1), throwTime).SetEase(Ease.InQuad);

    }

    public void ThrowDone()
    {
        Destroy(this.gameObject);
    }
}
