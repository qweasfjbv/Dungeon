using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManagerEx
{
    private bool isInEvent = false;

    public delegate void EventDelegate<T1>(T1 a);
    public EventDelegate<int> dialogDelegate;

    public void Init()
    {
        Managers.Input.dialogAction -= OnKeyboard;
        Managers.Input.dialogAction += OnKeyboard;
    }

    private void OnKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int eventIdx = 1;

            //�̺�ƮIdx �����ϴ� ���� �ʿ�

            dialogDelegate(eventIdx);
        }


        return;
    }


}
