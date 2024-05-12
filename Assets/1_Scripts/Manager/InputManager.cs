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

    // �� Block�� key�� �������� toggle �ǵ���
    // �ʿ��ϸ� �Լ������� ������Ƽ�� ���
    private bool dialogBlock = false;
    private bool escBlock = false;
    private bool invenBlock = false;
    private bool treasureBlock = false;

    public bool DialogBlock { get => dialogBlock; set => dialogBlock = value; }
    public bool EscBlock { get => escBlock; set => escBlock = value; }
    public bool InvenBlock { get => invenBlock; set => invenBlock = value; }
    public bool TreasureBlock{ get => treasureBlock; set => treasureBlock = value; }


    public void OnUpdate()
    {
        if (Input.anyKey == false) return;

        if (cardAction != null && !escBlock && !dialogBlock && !invenBlock && !treasureBlock) cardAction.Invoke();
        if (dialogAction != null && !escBlock && !invenBlock && !treasureBlock) dialogAction.Invoke();
        if (escAction != null && !dialogBlock && !invenBlock && !treasureBlock) escAction.Invoke();
        if (invenAction != null && !dialogBlock && !escBlock && !treasureBlock) invenAction.Invoke();
        if (treasureAction != null && !dialogBlock && !escBlock && !invenBlock) treasureAction.Invoke();

    }

}
