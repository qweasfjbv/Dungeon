using EnemyAI.BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEx
{
    public delegate void EventDelegate<T1>(T1 a);

    private bool isInEvent = false;
    private Action onEventStart = null;
    private Action onEventEnd = null;
    private EventDelegate<int> dialogDelegate;

    private List<BTree> goblins = new List<BTree>();

    public Action OnEventStartAction { get => onEventStart; set => onEventStart = value; }
    public Action OnEventEndAction { get=>onEventEnd; set => onEventEnd = value; }
    public EventDelegate<int> DialogDelegate {get=> dialogDelegate; set=>dialogDelegate = value; }


    public void Init()
    {
        Managers.Input.dialogAction -= OnKeyboard;
        Managers.Input.dialogAction += OnKeyboard;
    }

    public void NextEvent()
    {
        if (isInEvent) return;
        isInEvent = true;


        int eventIdx = 0;

        // TODO : 이벤트 선택지 선택하는 로직 필요
        onEventStart.Invoke();

        DialogDelegate(eventIdx);
    }

    // Event끝나면 호출
    public void OnEventEnd()
    {
        onEventEnd.Invoke();
        isInEvent = false;
        SoundManager.Instance.ChangeBGM(Define.BgmType.Main);
        RemoveAllGoblin();
    }

    private void OnKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            NextEvent();
        }

        return;
    }

    public void AddGoblin(BTree bt)
    {
        goblins.Add(bt);
    }

    private void RemoveAllGoblin()
    {
        for (int i = 0; i < goblins.Count; i++)
        {
            if (goblins[i] != null)
                GameObject.Destroy(goblins[i].gameObject);
        }

        goblins.Clear();
    }

}
