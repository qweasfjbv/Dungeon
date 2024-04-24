using EnemyUI.BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEx
{
    private bool isInEvent = false;

    public delegate void EventDelegate<T1>(T1 a);
    public EventDelegate<int> dialogDelegate;
    private Action onEventStart = null;

    public Action OnEventStart { get => onEventStart; set => onEventStart = value; }    
    public Action onEventEnd = null;

    private Dictionary<int, int> buffs = new Dictionary<int, int>();
    private List<BTree> goblins = new List<BTree>();

    public void Init()
    {
        Managers.Input.dialogAction -= OnKeyboard;
        Managers.Input.dialogAction += OnKeyboard;
    }

    public void NextEvent()
    {
        if (isInEvent) return;
        isInEvent = true;


        int eventIdx = 1;

        //이벤트Idx 선택하는 로직 필요
        onEventStart.Invoke();

        dialogDelegate(eventIdx);
    }

    // Event끝나면 호출
    public void OnEventEnd()
    {
        Debug.Log("ON END");
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
