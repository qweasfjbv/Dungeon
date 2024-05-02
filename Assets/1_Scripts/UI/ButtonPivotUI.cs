using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPivotUI : MonoBehaviour
{
    private float buttonHidePosX = 120f;
    private float buttonShowPosX = -50;


    private void Start()
    {
        ShowButton();
    }

    public void HideButton()
    {
        GetComponent<RectTransform>().DOAnchorPosX(buttonHidePosX, 0.7f).SetEase(Ease.OutCubic);
    }

    public void ShowButton()
    {
        GetComponent<RectTransform>().DOAnchorPosX(buttonShowPosX, 0.7f).SetEase(Ease.OutBounce);
    }

}