using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public abstract class CardBase : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
{

    private Transform parentDeck;

    public static string spritePath = "Sprites/Card/";

    public static string effectPath = "Sprites/Effect/";

    [SerializeField] private int cardId;
    [SerializeField] private int cardCost;
    [SerializeField] private Define.CardType cardType;

    [SerializeField] private float duration;
    [SerializeField] private float value;

    [SerializeField] private string effectName;

    private float cardMoveSpeed = 6f;

    [SerializeField]
    private Vector2 targetPos;
    private Vector2 targetScale;
    private float targetAngle;

    public static readonly float CARD_WIDTH = 200 * Settings.xScale;
    public static readonly float CARD_HEIGHT = 300 * Settings.yScale;
    public static readonly float CARD_SCALE = 1.0f;
    public static readonly float CARD_SCALE_HOVERED = 1.3f;

    private bool isHover = false;
    private bool isDragged = false;
    private bool isInField = false;
    public bool IsHover { get=>isHover; }
    public bool IsDragged { get => isDragged; }
    public bool IsInField { get => isInField; }

    private RectTransform rect;

    #region Setter
    public void SetTargetPosX(float x)
    {
        targetPos.x = x;
    }

    public void SetTargetPosY(float y)
    {
        targetPos.y = y;
    }

    public float GetTargetPosY()
    {
        return targetPos.y;
    }

    public void SetTargetScaleX(float x)
    {
        targetScale.x = x;
    }

    public void SetTargetScaleY(float y)
    {
        targetScale.y = y;
    }

    public void SetTargetAngle(float a)
    {
        targetAngle = a;
    }
    #endregion


    public void SetCard(int id)
    {
        var cardInfo = Managers.Resource.GetCardInfo(id);

        this.cardId = cardInfo.cardId;
        this.cardCost = cardInfo.cardCost;
        this.cardType = cardInfo.cardType;

        // ī�� UI�� ��������Ʈ ����
        Debug.Log("cardInfo : " + cardInfo.spriteName);
        effectName = cardInfo.effectName;
    }

    // ���� ����� �� ȣ���� �Լ�
    public abstract void ActivateEffect(Vector3 pos);

    private void Update()
    {
        var targetV = MathHelper.CardLerp(rect.anchoredPosition, targetPos, 6f);
        this.rect.anchoredPosition = new Vector3(targetV.x, targetV.y);
        this.rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, 0);

        this.rect.localScale= MathHelper.CardLerp(rect.localScale, targetScale, cardMoveSpeed);

        var rotateZ = rect.localRotation.eulerAngles.z;
        if (rotateZ >= 180) rotateZ = rotateZ - 360;

        this.rect.localRotation = Quaternion.Euler(0, 0, MathHelper.CardRotateLerp(rotateZ, targetAngle, 6f));


    }

    private void OnEnable()
    {
        parentDeck = this.transform.parent;
        rect = GetComponent<RectTransform>();
        targetPos.x = 0;
        targetPos.y = 0;
        targetScale.x = rect.localScale.x;
        targetScale.y = rect.localScale.y;
        targetAngle = 0;
    }


    private void Hover()
    {
        if (!isHover)
        {
            isHover = true;
            targetScale = new Vector3(CARD_SCALE_HOVERED, CARD_SCALE_HOVERED);
        }
    }

    private void UnHover()
    {
        if (isHover)
        {
            isHover = false;
            targetScale = new Vector3(CARD_SCALE, CARD_SCALE);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hover();
        if(transform.parent != null)
        {
            transform.parent.GetComponent<CardInHand>().OnHover();
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        UnHover();
        if (transform.parent != null)
        {
            transform.parent.GetComponent<CardInHand>().OnUnHover();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {

        var mousePos = transform.parent.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        SetTargetPosX(mousePos.x);
        SetTargetPosY(mousePos.y);

        var tmpColor = GetComponent<Image>().color;
        if (mousePos.y > CARD_HEIGHT)
        {
            isInField = true;
            tmpColor.a = MathHelper.ColorAlphaLerp(tmpColor.a, 0f, 10f);
            GetComponent<Image>().color = tmpColor;
        }
        else
        {
            isInField = false;
            tmpColor.a = MathHelper.ColorAlphaLerp(tmpColor.a, 1f, 6f);
            GetComponent<Image>().color = tmpColor;
        }

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragged = true;
        transform.parent.GetComponent<CardInHand>().OnUnHover();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isInField)
        {
            // TODO : EFFECT �ʿ�

            isDragged = false;
            transform.parent.GetComponent<CardInHand>().RemoveCardInHand(transform.GetSiblingIndex());
        }
        else
        {

            isDragged = false;

            GetComponent<Image>().color = Color.white;
            transform.parent.GetComponent<CardInHand>().OnUnHover();
        }

    }

    private void OnDestroy()
    {
        parentDeck.GetComponent<CardInHand>().UpdateCardLayout();
    }
}
