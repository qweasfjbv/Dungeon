using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEx
{
    private bool isInEvent = false;

    public delegate void EventDelegate<T1>(T1 a);
    public EventDelegate<int> dialogDelegate;
    public Action onEventStart = null;
    public Action onEventEnd = null;


    public void Init()
    {
        Managers.Input.dialogAction -= OnKeyboard;
        Managers.Input.dialogAction += OnKeyboard;
    }

    public void NextEvent()
    {

        int eventIdx = 1;

        //�̺�ƮIdx �����ϴ� ���� �ʿ�
        onEventStart.Invoke();

        dialogDelegate(eventIdx);
    }

    // Event������ ȣ��
    public void OnEventEnd()
    {
        onEventEnd.Invoke();
    }

    private void OnKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            NextEvent();
        }


        return;
    }


}
