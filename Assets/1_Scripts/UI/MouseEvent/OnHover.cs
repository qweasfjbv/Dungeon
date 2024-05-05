using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TooltipDir {Left, Down}

public class OnHover : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
    , IPointerUpHandler
    , IPointerDownHandler
{

    Vector3 originScale;

    private const float PADDING = 30f;

    [SerializeField]
    private GameObject tooltip;
    [SerializeField]
    private TooltipDir ttDir;

    private void Start()
    {
        originScale = transform.GetComponent<RectTransform>().localScale;
        if (tooltip != null)
            tooltip.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.GetComponent<RectTransform>().localScale = originScale * 1.3f;
        if (tooltip != null)
        {
            if (ttDir == TooltipDir.Left)
                tooltip.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition - new Vector2(GetComponent<RectTransform>().sizeDelta.x + PADDING, 0);
            else
                tooltip.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition - new Vector2(0, GetComponent<RectTransform>().sizeDelta.y + PADDING);
            tooltip.SetActive(true);
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        transform.GetComponent<RectTransform>().localScale = originScale;

        if (tooltip != null)
        {
            tooltip.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SoundManager.Instance.PlayButtonSound(Define.ButtonSoundType.ClickButton);
        transform.GetComponent<RectTransform>().localScale = originScale * 0.7f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {

        transform.GetComponent<RectTransform>().localScale = originScale * 1.3f;
    }
}
