using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnHover : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
{

    Vector3 originScale;

    private const float PADDING = 10f;

    [SerializeField]
    private GameObject tooltip;

    private void Start()
    {
        originScale = transform.GetComponent<RectTransform>().localScale;
        tooltip.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.GetComponent<RectTransform>().localScale = originScale * 1.3f;
        if (tooltip != null)
        {

            tooltip.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition - new Vector2(GetComponent<RectTransform>().sizeDelta.x + PADDING, 0);
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
}
