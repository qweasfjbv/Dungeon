using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPivotUI : MonoBehaviour
{
    private Vector2 buttonHidePos = new Vector2(120, 30);
    private Vector2 buttonShowPos = new Vector2(-50, 30);


    private void Start()
    {
        ShowButton();
    }

    public void HideButton()
    {
        GetComponent<RectTransform>().DOAnchorPos(buttonHidePos, 0.7f).SetEase(Ease.OutCubic);
    }

    public void ShowButton()
    {
        GetComponent<RectTransform>().DOAnchorPos(buttonShowPos, 0.7f).SetEase(Ease.OutBounce);
    }

}