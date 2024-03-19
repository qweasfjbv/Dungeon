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
}
