using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InvenManager
{
    List<int> summonCardList = new List<int>();
    List<int> magicCardList = new List<int>();
    List<int> treasureList = new List<int>();

    List<int> unusedCardList = new List<int>();
    List<int> usedCardList = new List<int>();

    private const float DRAWCOST = 1;

    public Action onGameEnd;

    public void Init()
    {
        Managers.Game.onEventEnd -= OnGameEnd;
        Managers.Game.onEventEnd += OnGameEnd;


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

        Managers.Game.OnEventStart -= OnGameStart;
        Managers.Game.OnEventStart += OnGameStart;
        return;
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
        onGameEnd.Invoke();
        unusedCardList.Clear();
        usedCardList.Clear();
    }

}
