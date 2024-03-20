using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class CardBase : MonoBehaviour
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

        // 카드 UI에 스프라이트 적용
        Debug.Log("cardInfo : " + cardInfo.spriteName);
        effectName = cardInfo.effectName;
    }

    // 실제 사용할 때 호출할 함수
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
}
