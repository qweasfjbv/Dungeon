using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputManager
{
    public Action dialogAction = null;
    public Action cardAction = null;
    public Action escAction = null;
    public Action invenAction = null;
    public Action treasureAction = null;

    // 각 Block은 key를 눌렀을때 toggle 되도록
    // 필요하면 함수딴에서 프로퍼티로 토글
    private bool dialogBlock = false;
    private bool escBlock = false;
    private bool invenBlock = false;

    public bool DialogBlock { get => dialogBlock; set => dialogBlock = value; }
    public bool EscBlock { get => escBlock; set => escBlock = value; }
    public bool InvenBlock { get => invenBlock; set => invenBlock = value; }


    public void OnUpdate()
    {
        if (Input.anyKey == false) return;

        if (cardAction != null && !escBlock && !dialogBlock && !invenBlock) {
            cardAction.Invoke();
            treasureAction.Invoke();
        }
        if (dialogAction != null && !escBlock && !invenBlock) dialogAction.Invoke();
        if (escAction != null && !dialogBlock && !invenBlock) escAction.Invoke();
        if (invenAction != null && !dialogBlock && !escBlock) invenAction.Invoke();

    }

}
