using JetBrains.Annotations;
using System;
using System.Diagnostics.Tracing;
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


[Serializable]
public class DialogEventInfo {
    public int eventId;
    public string eventName;
    public int eventDialogCnt;
    public int eventSelectCnt;
}

[Serializable]
public class EventInfos {
    public DialogEventInfo[] eventInfo;
}

public class ResourceManager
{

    private string cardInfoPath = "JsonData/CardData";
    private string eventInfoPath = "JsonData/EventData";

    // json->리소스 받아오기, 배열에 저장
    private CardInfos cardInfos = new CardInfos();
    private EventInfos eventInfos = new EventInfos();
    private GameObject[] cardPrefabs;

    const int CARDOFFSET = 11;

    public void Init()
    {
        cardInfos = JsonUtility.FromJson<CardInfos>(Resources.Load<TextAsset>(cardInfoPath).text);
        eventInfos = JsonUtility.FromJson<EventInfos>(Resources.Load<TextAsset>(eventInfoPath).text);


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

    public DialogEventInfo GetEventInfo(int id)
    {
        return eventInfos.eventInfo[id];
    }
    
}
