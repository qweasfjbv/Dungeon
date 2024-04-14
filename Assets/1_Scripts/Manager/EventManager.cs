using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EventManager : MonoBehaviour
{




    static EventManager s_instance;
    public static EventManager Instance { get { return s_instance; } }

    private Dictionary<(int eventId, int choice), Action> eventActions;



    private void Awake()
    {
        s_instance = gameObject.GetComponent<EventManager>();
    }


    void Start()
    {
        // 이벤트 함수 매핑 초기화
        eventActions = new Dictionary<(int, int), Action>
        {
            { (1, 0), GG_S0 },
            { (1, 1), GG_S1 },
            { (2, 0), WT_S0 },
            { (2, 1), WT_S1 },
            { (2, 2), WT_S2 }
        };
    }

    public void EventSelectTrigger(int id, int select)
    {
        eventActions[(id, select)].Invoke();
    }

    private void GG_S0()
    {
        Debug.Log("GG_S0");

        Managers.Game.OnEventEnd();
    }

    private void GG_S1()
    {
        Debug.Log("GG_S1");

        Managers.Game.OnEventEnd();
    }
    private void WT_S0()
    {

        Managers.Game.OnEventEnd();
    }
    private void WT_S1()
    {

        Managers.Game.OnEventEnd();
    }

    private void WT_S2()
    {

        Managers.Game.OnEventEnd();
    }

}
