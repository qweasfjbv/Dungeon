using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnHover_2 : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
{

    private Transform icon;

    private bool isClicked = false;

    private float originX;
    private float originY;

    private float targetX;
    private float targetY;

    private Color targetColor = Color.white;
    private float targetAlpha = 1f;

    private float onHoverOffset = -40f;

    public void OnSelected()
    {
        if (!isClicked)
        {
            isClicked = true;
            targetAlpha = 0.3f;
        }
    }

    public void OnUnselected()
    {
        if (isClicked)
        {
            isClicked = false;
            targetAlpha = 1f;
        }
    }

    private void Awake()
    {
        originX = targetX = GetComponent<RectTransform>().anchoredPosition.x;
        originY = targetY = GetComponent<RectTransform>().anchoredPosition.y;


        icon = transform.GetChild(0);
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetX = originX + onHoverOffset;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetX = originX;
    }

    

    // Update is called once per frame
    void Update()
    {
        GetComponent<RectTransform>().anchoredPosition = UtilFunctions.CardLerp(GetComponent<RectTransform>().anchoredPosition, new Vector2(targetX, targetY), 6.0f);
        targetColor.a = UtilFunctions.ColorAlphaLerp(targetColor.a, targetAlpha, 6f, 0.01f);
        GetComponent<Image>().color = targetColor;
        icon.GetComponent<Image>().color = targetColor;
    }


}
