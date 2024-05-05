using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvenPivotUI : MonoBehaviour
{
    private float buttonHidePosY = 120f;
    private float buttonShowPosY = 0;


    private void Start()
    {
        ShowButton();
    }

    public void HideButton()
    {
        GetComponent<RectTransform>().DOAnchorPosY(buttonHidePosY, 0.7f).SetEase(Ease.OutCubic);
    }

    public void ShowButton()
    {
        GetComponent<RectTransform>().DOAnchorPosY(buttonShowPosY, 0.7f).SetEase(Ease.OutBounce);
    }

}
