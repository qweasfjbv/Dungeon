using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvenManager
{
    List<int> summonCardList = new List<int>();
    List<int> magicCardList = new List<int>();
    List<int> treasureList = new List<int>();

    List<int> unusedCardList = new List<int>();
    List<int> usedCardList = new List<int>();

    private const float DRAWCOST = 1;
    
    private GameObject invenParent;

    public Action onGameEnd;

    public void Init()
    {


        Managers.Game.OnEventEndAction -= OnGameEnd;
        Managers.Game.OnEventEndAction += OnGameEnd;

        invenParent = GameObject.Find("InvenContent");

        Managers.Input.escAction -= TmpKey;
        Managers.Input.escAction += TmpKey;


        Managers.Game.OnEventStartAction -= OnGameStart;
        Managers.Game.OnEventStartAction += OnGameStart;
        return;
    }
    
    private void TmpKey()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddCard(1);
        }
    }
    public void OnGameStart()
    {
        foreach (var card in summonCardList)
            unusedCardList.Add(card);
        foreach (var card in magicCardList)
            unusedCardList.Add(card);
    }

    public int DrawCard()
    {
        if (unusedCardList.Count == 0) return -1;
        if (!SliderController.Instance.UseMana(DRAWCOST)) return -2;

        int cardIdx = UnityEngine.Random.Range(0, unusedCardList.Count - 1);
        int retid = unusedCardList[cardIdx];

        unusedCardList.RemoveAt(cardIdx);


        return retid;
    }

    public void MoveUsedtoUnused()
    {
        // 카드 초기화할때도 드로우할 떄 만큼 마나 소비
        if (!SliderController.Instance.UseMana(DRAWCOST))
        {
            Debug.Log("Mana부족");
            return;
        }
        foreach (var used in usedCardList)
        {
            unusedCardList.Add(used);
        }

        usedCardList.Clear();
    }

    public void AddCard(int id)
    {
        Debug.Log("ADDED");
        var cardData = Managers.Resource.GetCardInfo(id);

        switch (cardData.cardType)
        {
            case Define.CardType.Summon:
                summonCardList.Add(id);
                break;
            case Define.CardType.Magic:
                magicCardList.Add(id);
                break;
        }

        GameObject.Instantiate(Managers.Resource.GetCardPrefab(cardData.cardId), invenParent.transform); 
    }
    public void OnUseCard(int id)
    {
        usedCardList.Add(id);
    }

    public void OnGameEnd()
    {
        onGameEnd.Invoke();
        unusedCardList.Clear();
        usedCardList.Clear();
    }

}
