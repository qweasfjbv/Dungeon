using EnemyUI.BehaviorTree;
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
        monster.GetComponent<GoblinBT>().enabled = true;
        monster.GetComponent<Animator>().SetBool("Idle", true);
        monster.transform.position = pos;
        monster.transform.GetComponent<SpriteRenderer>().color = Color.white;
        // 움직이는 스크립트 (BT) 켜줘야함
    }

    public override void PreviewEffect(Vector3 pos)
    {

        pos.z = 0;

        monster.transform.position = pos;
        monster.GetComponent<Collider2D>().enabled = false;
        monster.GetComponent<GoblinBT>().enabled = false;
        Color tmpC = monster.transform.GetComponent<SpriteRenderer>().color;
        tmpC.a = 0.5f;
        monster.transform.GetComponent<SpriteRenderer>().color = tmpC;

        // BT 꺼줘야함

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
