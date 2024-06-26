using EnemyAI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCard : CardBase
{
    [SerializeField]
    private GameObject monsterPrefab;

    private GameObject monster = null;


    public override void ActivateEffect(Vector3 pos)
    {
        SoundManager.Instance.PlaySfxSound(Define.SFXSoundType.Place);
        pos.z = 0;
        monster.GetComponent<Collider2D>().enabled = true;
        monster.GetComponent<BTree>().enabled = true;
        monster.GetComponent<Animator>().SetBool("Idle", true);
        monster.transform.position = new Vector3(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f, 0);
        monster.transform.GetComponent<SpriteRenderer>().color = Color.white;

        Managers.Game.AddGoblin(monster.GetComponent<BTree>());
        // �����̴� ��ũ��Ʈ (BT) �������
    }

    public override void PreviewEffect(Vector3 pos)
    {

        pos.z = 0;

        monster.transform.position = pos;
        monster.GetComponent<Collider2D>().enabled = false;
        monster.GetComponent<BTree>().enabled = false;
        Color tmpC = monster.transform.GetComponent<SpriteRenderer>().color;
        tmpC.a = 0.5f;
        monster.transform.GetComponent<SpriteRenderer>().color = tmpC;

        // BT �������

        if (!monster.activeSelf)
            monster.SetActive(true);

    }

    public override void UnPreviewEffect()
    {

        monster.SetActive(false);
    }


    public override void OnEnable()
    {
        base.OnEnable();
        if (monster == null)
        {
            monster = Instantiate(monsterPrefab);
            monster.SetActive(false);
        }

    }

    public override bool PayCardCost()
    {
        if (SliderController.Instance.UseBlood(cardCost)) return true;
        else return false;
    }
}
