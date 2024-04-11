using UnityEngine;
using DG.Tweening;

public class OptionUI : MonoBehaviour
{




    #region TOGGLE
    private bool isOnUI = false;


    private Vector2 hidePos = new Vector2(0, 1000);
    private Vector2 showPos = new Vector2(0, 0);

    // True면 킨, false면 끈 상태
    public bool Toggle()
    {
        if (!isOnUI)
        {
            isOnUI = true;
            GetComponent<RectTransform>().DOAnchorPos(showPos, 0.7f).SetEase(Ease.InOutElastic);
            return true;
        }
        else
        {
            isOnUI = false;
            GetComponent<RectTransform>().DOAnchorPos(hidePos, 0.7f).SetEase(Ease.InOutElastic);
            return false;
        }
    }

    #endregion
}
