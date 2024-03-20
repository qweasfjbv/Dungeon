using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class CardBase : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
    , IDragHandler
{

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
    public bool IsHover { get=>isHover; }

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

    }
}
