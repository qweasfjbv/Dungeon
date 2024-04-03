using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnHover : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
{

    Vector3 originScale;

    private void Start()
    {
        originScale = transform.GetComponent<RectTransform>().localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.GetComponent<RectTransform>().localScale = originScale * 1.3f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.GetComponent<RectTransform>().localScale = originScale;
    }
}
