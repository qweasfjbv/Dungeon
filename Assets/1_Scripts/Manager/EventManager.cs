using EnemyAI.BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    static EventManager s_instance;
    public static EventManager Instance { get { return s_instance; } }

    private const int MAX_QUOTA = 20;
    private int curQuota = 0;
    private int passedEnemyCount = 0;

    private Dictionary<(int eventId, int choice), Action> eventActions;

    [SerializeField] private TreasureController tController;
    [SerializeField] private TextMeshProUGUI quotaText;
    [SerializeField] private CardMerchant merchant;

    private void Awake()
    {
        s_instance = gameObject.GetComponent<EventManager>();
    }

    public void EnemyPassed()
    {
        passedEnemyCount++;
    }
    public void QuotaProgressed()
    {
        curQuota++; UpdateQuotaText();

        if (curQuota >= MAX_QUOTA)
        {
            // TODO : �Ҵ緮 ä��. ���� �̺�Ʈ ���� ex. boss
        }
    }

    void Start()
    {
        merchant.gameObject.SetActive(false);
        curQuota = 0;
        UpdateQuotaText();

        Managers.Game.OnEventStartAction -= (InitEvent);
        Managers.Game.OnEventStartAction += (InitEvent);

        Managers.Game.OnPositiveEventEndAction -= (QuotaProgressed);
        Managers.Game.OnPositiveEventEndAction += (QuotaProgressed);

        // �̺�Ʈ �Լ� ���� �ʱ�ȭ
        eventActions = new Dictionary<(int, int), Action>
        {
            { (0, 0), F_0_0 },
            { (0, 1), F_0_1 },
            { (1, 0), F_1_0 },
            { (1, 1), F_1_1 },
            { (2, 0), F_2_0 },
            { (2, 1), F_2_1 },
            { (3, 0), F_3_0 },
            { (3, 1), F_3_1 }
        };
    }

    private void UpdateQuotaText()
    {
        quotaText.text = curQuota + " / " + MAX_QUOTA.ToString();
    }

    private void InitEvent()
    {
        passedEnemyCount = 0;
    }

    public void EventSelectTrigger(int id, int select)
    {
        eventActions[(id, select)].Invoke();
    }

    // Ȳ�� ���
    private void F_0_0()
    {
        SoundManager.Instance.PlayEffectSound(Define.EffectSoundType.Coin);
        tController.AddItemList(0);
        Managers.Game.OnPositiveEventEnd();
    }

    private void F_0_1()
    {
        Managers.Game.OnEventEnd();
    }

    // ����
    private void F_1_0()
    {
        merchant.SetMerchant(Define.CardType.Magic);
        Managers.Game.OnEventEnd();
    }
    private void F_1_1()
    {
        // 
        Managers.Game.OnEventEnd();
    }

    // ����_����
    private void F_2_0()
    {
        merchant.SetMerchant(Define.CardType.Summon);
    }

    // ����_����
    private void F_2_1()
    {
        Managers.Game.OnEventEnd();
    }


    // ���� - ����
    private void F_3_0()
    {
        StartCoroutine(EnemyAppearEvent(3));
    }

    // ���� - ����
    private void F_3_1()
    {
        // TODO : ���� ü�� ���
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
                if (enemyBTs[i].gameObject.activeSelf == true) once = true;
            }

            yield return new WaitForSeconds(1.0f);

            if (!once) break;
        }


        Managers.Game.OnPositiveEventEnd();
        enemyBTs.Clear();

        if (passedEnemyCount > 0)
        {
            // TODO : �����ѹ�
            Managers.Game.SelectEvent(4);
        }
    }
}
