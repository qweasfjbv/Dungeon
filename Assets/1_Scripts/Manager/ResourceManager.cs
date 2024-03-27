using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class CardInfo
{
    public int cardId;
    public int cardCost;
    public Define.CardType cardType;

    public float duration;
    public float value;

    public string cardName;
    public string cardDesc;
}

[Serializable]
public class CardInfos
{
    public CardInfo[] cardInfo;
}

public class ResourceManager
{

    private string cardInfoPath = "JsonData/CardData";

    // json->���ҽ� �޾ƿ���, �迭�� ����
    private CardInfos cardInfos = new CardInfos();
    private GameObject[] cardPrefabs;

    const int CARDOFFSET = 11;

    public void Init()
    {
        cardInfos = JsonUtility.FromJson<CardInfos>(Resources.Load<TextAsset>(cardInfoPath).text);


        cardPrefabs = Resources.LoadAll<GameObject>("Prefabs/Card");
    }

    public CardInfo GetCardInfo(int id)
    {
        return cardInfos.cardInfo[id - CARDOFFSET];
    }

    public GameObject GetCardPrefab(int id)
    {
        return cardPrefabs[id - CARDOFFSET];
    }

    
}
