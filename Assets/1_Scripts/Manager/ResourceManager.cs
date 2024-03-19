using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class CardInfo
{
    public int cardId;
    public int cardCost;
    public Define.CardType cardType;

    public float duration;
    public float value;

    public string spriteName;
    public string effectName;
}

public class CardInfos
{
    public CardInfo[] cardInfo;
}

public class ResourceManager
{

    private string cardInfoPath = "JsonData/CardData";

    // json->���ҽ� �޾ƿ���, �迭�� ����
    private CardInfos cardInfos = new CardInfos();

    public void Init()
    {
        cardInfos = JsonUtility.FromJson<CardInfos>(Resources.Load<TextAsset>(cardInfoPath).text);
        Debug.Log(Resources.Load<TextAsset>(cardInfoPath).text);
    }

    public CardInfo GetCardInfo(int id)
    {
        return cardInfos.cardInfo[id];
    }

    
}
