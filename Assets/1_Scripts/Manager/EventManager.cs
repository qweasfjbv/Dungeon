using EnemyAI.BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
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
            { (0, 0), F_0_0 },
            { (0, 1), F_0_1 },
            { (0, 2), F_0_2 },
            { (1, 0), F_1_0 },
            { (1, 1), F_1_1 }
        };
    }

    public void EventSelectTrigger(int id, int select)
    {
        eventActions[(id, select)].Invoke();
    }

    private void F_0_0()
    {
        Debug.Log("GG_S0");
        StartCoroutine(EnemyAppearEvent(3));
    }

    private void F_0_1()
    {
        Debug.Log("GG_S1");
        Managers.Inven.AddRandomCard();
        Managers.Game.OnEventEnd();
    }
    private void F_0_2()
    {
        Debug.Log("GG_S1");

        Managers.Game.OnEventEnd();
    }
    private void F_1_0()
    {

        Managers.Game.OnEventEnd();
    }
    private void F_1_1()
    {

        Managers.Game.OnEventEnd();
    }

    private IEnumerator EnemyAppearEvent(int cnt)
    {

        SoundManager.Instance.ChangeBGM(Define.BgmType.Game);
        List<EnemyBT> enemyBTs = new List<EnemyBT>();

        for (int i = 0; i < cnt; i++)
        {
            enemyBTs.Add(MapGenerator.Instance.SummonEnemy());
            yield return new WaitForSeconds(0.3f);
        }

        bool once;
        while (true)
        {
            once = false;
            for (int i = 0; i < cnt; i++)
            {
                if (enemyBTs[i] != null) once = true;
            }

            yield return new WaitForSeconds(1.0f);

            if (!once) break;
        }


        Managers.Game.OnEventEnd();
        enemyBTs.Clear();
    }
}
