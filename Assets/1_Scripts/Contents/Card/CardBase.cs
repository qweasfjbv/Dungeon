using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public abstract class CardBase : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
    , IPointerDownHandler
{

    private Transform parentDeck;

    public static string spritePath = "Sprites/Card/";

    public static string effectPath = "Sprites/Effect/";

    protected int cardId;
    protected int cardCost;
    private Define.CardType cardType;

    private float duration;
    private float value;

    private bool isInHand = false;

    [SerializeField] protected Sprite itemSprite;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI cardDesc;
    [SerializeField] private TextMeshProUGUI cardCostText;



    private float cardMoveSpeed = 6f;

    private Vector2 targetPos;
    private Vector2 targetScale;
    private float targetAngle;

    public static readonly float CARD_WIDTH = 100 * Settings.xScale;
    public static readonly float CARD_HEIGHT = 150 * Settings.yScale;
    public static readonly float CARD_SCALE = 1.0f;
    public static readonly float CARD_SCALE_HOVERED = 1.5f;

    private bool isHover = false;
    private bool isDragged = false;
    private bool isInField = false;
    public bool IsHover { get => isHover; }
    public bool IsDragged { get => isDragged; }
    public bool IsInField { get => isInField; }
    public Define.CardType Cardtype{ get => cardType; }

    private RectTransform rect;

    #region Getter/Setter

    public int CardID { get => cardId; }
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
    public float GetTargetPosX()
    {
        return targetPos.x;
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


    public void SetCard(int id, bool toHand = true)
    {

        isInHand = toHand;

        var cardInfo = Managers.Resource.GetCardInfo(id);

        this.cardId = cardInfo.cardId;
        this.cardCost = cardInfo.cardCost;
        this.cardType = cardInfo.cardType;

        cardCostText.text = cardInfo.cardCost.ToString();
        cardName.GetComponent<LocalizeStringEvent>().StringReference.SetReference("CardInfoTable", (cardInfo.cardId * 100 + 0).ToString());
        cardDesc.GetComponent<LocalizeStringEvent>().StringReference.SetReference("CardInfoTable", (cardInfo.cardId * 100 + 1).ToString());

    }

    public void UnsetCard()
    {
        isInHand = false;
    }

    // 실제 사용할 때 호출할 함수
    public abstract void ActivateEffect(Vector3 pos);
    public abstract void PreviewEffect(Vector3 pos);
    public abstract void UnPreviewEffect();
    public abstract bool PayCardCost();

    private void Update()
    {
        this.rect.localScale= UtilFunctions.CardLerp(rect.localScale, targetScale, cardMoveSpeed);

        if (!isInHand) return;
        var targetV = UtilFunctions.CardLerp(rect.anchoredPosition, targetPos, cardMoveSpeed);
        this.rect.anchoredPosition = new Vector3(targetV.x, targetV.y);
        this.rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, 0);

        var rotateZ = rect.localRotation.eulerAngles.z;
        if (rotateZ >= 180) rotateZ = rotateZ - 360;

        this.rect.localRotation = Quaternion.Euler(0, 0, UtilFunctions.CardRotateLerp(rotateZ, targetAngle, cardMoveSpeed));


    }

    public void OnUsable()
    {

    }
    
    public void OnUnusable()
    {

    }

    #region LifeCycle
    public virtual void OnEnable()
    {
        SoundManager.Instance.PlaySfxSound(Define.SFXSoundType.Paper);
        parentDeck = this.transform.parent;
        rect = GetComponent<RectTransform>();
        targetPos.x = 0;
        targetPos.y = 0;
        targetScale.x = rect.localScale.x;
        targetScale.y = rect.localScale.y;
        targetAngle = 0;
    }

    private void OnDestroy()
    {
        if (isInHand)
            parentDeck.GetComponent<CardInHand>().UpdateCardLayout();
    }

    #endregion


    #region IHandler
    private void Hover()
    {
        if (!isHover)
        {
            isHover = true;
            rect.localScale = new Vector3(CARD_SCALE_HOVERED, CARD_SCALE_HOVERED);
            targetScale = new Vector3(CARD_SCALE_HOVERED, CARD_SCALE_HOVERED);
            SetTargetAngle(0f);
            if (!isDragged)
            {
                SetTargetPosY(CardBase.CARD_SCALE_HOVERED * CardBase.CARD_HEIGHT / 1.5f);
            }
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
        if(transform.parent != null && isInHand)
        {
            transform.parent.GetComponent<CardInHand>().OnHover();
        }

    }


    public void OnPointerExit(PointerEventData eventData)
    {
        UnHover();
        if (transform.parent != null && isInHand)
        {
            transform.parent.GetComponent<CardInHand>().OnUnHover();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {

        if (!isInHand) return;

        var mousePos = transform.parent.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        SetTargetPosX(mousePos.x);
        SetTargetPosY(mousePos.y);

        var tmpColor = transform.GetChild(0).GetComponent<Image>().color;
        
        if (mousePos.y > CARD_HEIGHT)
        {
            isInField = true;
            PreviewEffect(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            tmpColor.a = UtilFunctions.ColorAlphaLerp(tmpColor.a, 0f, 2* cardMoveSpeed);
            SetColor(tmpColor);
        }
        else
        {
            isInField = false;
            tmpColor.a = UtilFunctions.ColorAlphaLerp(tmpColor.a, 1f, cardMoveSpeed);
            SetColor(tmpColor);
        }

    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if (!isInHand) return;

        SoundManager.Instance.PlaySfxSound(Define.SFXSoundType.Paper);
        CameraController.CanMove = false;
        isDragged = true;
        transform.parent.GetComponent<CardInHand>().UpdateCardLayout();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isInHand) return;

        CameraController.CanMove = true;
        isDragged = false;
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (isInField && MapGenerator.Instance.GetGridType(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y)) != Define.GridType.None
            && PayCardCost())
        {
            
            // TODO : EFFECT 필요
            ActivateEffect(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            transform.parent.GetComponent<CardInHand>().RemoveCardInHand(transform.GetSiblingIndex());
        }
        else
        {
            UnPreviewEffect();
            SetColor(Color.white);
            transform.parent.GetComponent<CardInHand>().UpdateCardLayout();
        }

    }

    private void SetColor(Color color)
    {
        foreach(Transform child in transform)
        {
            var img = child.GetComponent<Image>();
            if (img != null) {
                img.color = color;
            }

            var text = child.GetComponent<TextMeshProUGUI>();
            if(text != null)
            {
                text.color = new Color(0,0, 0, color.a);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    #endregion

}
