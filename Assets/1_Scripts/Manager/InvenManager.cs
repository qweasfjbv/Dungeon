using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvenManager
{
    List<int> summonCardList = new List<int>();
    List<int> magicCardList = new List<int>();
    List<int> buffCardList = new List<int>();
    List<int> treasureList = new List<int>();

    List<int> unusedCardList = new List<int>();
    List<int> usedCardList = new List<int>();

    private const float DRAWCOST = 1;

    public void Init()
    {
        summonCardList.Add(4);
        summonCardList.Add(5);
        summonCardList.Add(4);
        summonCardList.Add(5);
        summonCardList.Add(4);
        summonCardList.Add(5);
        summonCardList.Add(4);
        summonCardList.Add(5);


        magicCardList.Add(1);
        magicCardList.Add(2);
        magicCardList.Add(3);
        magicCardList.Add(1);
        magicCardList.Add(2);
        magicCardList.Add(3);
        magicCardList.Add(1);
        magicCardList.Add(2);
        magicCardList.Add(3);

        OnGameStart();
        return;
    }
    
    public void OnGameStart()
    {
        foreach (var card in summonCardList)
            unusedCardList.Add(card);
        foreach (var card in magicCardList)
            unusedCardList.Add(card);
        foreach (var card in buffCardList)
            unusedCardList.Add(card);
    }

    public int DrawCard()
    {
        if (unusedCardList.Count == 0) return -1;
        if (!SliderController.Instance.UseMana(DRAWCOST)) return -2;

        int cardIdx = Random.Range(0, unusedCardList.Count - 1);
        int retid = unusedCardList[cardIdx];
        unusedCardList.RemoveAt(cardIdx);

        return retid;
    }

    public void MoveUsedtoUnused()
    {
        foreach (var used in usedCardList)
        {
            unusedCardList.Add(used);
        }

        usedCardList.Clear();
    }

    public void OnUseCard(int id)
    {
        usedCardList.Add(id);
    }

    public void OnGameEnd()
    {
        unusedCardList.Clear();
        usedCardList.Clear();
    }

}