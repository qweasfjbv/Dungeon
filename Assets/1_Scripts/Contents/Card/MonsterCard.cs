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

        pos.z = 0;
        monster.transform.position = pos;
        monster.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
        // 움직이는 스크립트 (BT) 켜줘야함
    }

    public override void PreviewEffect(Vector3 pos)
    {

        pos.z = 0;

        monster.transform.position = pos;
        Color tmpC = monster.transform.GetChild(0).GetComponent<SpriteRenderer>().color;
        tmpC.a = 0.5f;
        monster.transform.GetChild(0).GetComponent<SpriteRenderer>().color = tmpC;

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
        return false;
    }
}
