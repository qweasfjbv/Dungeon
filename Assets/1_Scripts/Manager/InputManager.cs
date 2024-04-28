using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputManager
{
    public Action dialogAction = null;
    public Action cardAction = null;
    public Action escAction = null;

    // 각 Block은 key를 눌렀을때 toggle 되도록
    // 필요하면 함수딴에서 프로퍼티로 토글
    private bool dialogBlock = false;
    private bool escBlock = false;

    public bool DialogBlock { get => dialogBlock; set => dialogBlock = value; }
    public bool EscBlock { get => escBlock; set => escBlock = value; }

    public void OnUpdate()
    {
        if (Input.anyKey == false) return;

        if (cardAction != null && !escBlock && !dialogBlock) cardAction.Invoke();
        if (dialogAction != null && !escBlock) dialogAction.Invoke();
        if (escAction != null && !dialogBlock) escAction.Invoke();

        
    }

}
