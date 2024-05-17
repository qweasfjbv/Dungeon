using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCard : CardBase
{

    [SerializeField]
    private GameObject effectPrefab;

    private GameObject effect = null;


    public override void ActivateEffect(Vector3 pos)
    {
        pos.z = 0;
        effect.transform.position = pos;
    }


    public override void PreviewEffect(Vector3 pos)
    {
        pos.z = 0;

        effect.transform.position = pos;
        effect.GetComponent<PotionEffect>().PreviewizeEffect();

        if (!effect.activeSelf)
            effect.SetActive(true);
    }

    public override void UnPreviewEffect()
    {
        effect.SetActive(false);
    }

    // 이펙트 미리 생성
    public override void OnEnable()
    {
        base.OnEnable();
        if (effect == null)
        {
            effect = Instantiate(effectPrefab);
            effect.SetActive(false);
        }

    }
    public override bool PayCardCost()
    {
        if (SliderController.Instance.UseMana(cardCost)) return true;
        else return false;
    }
}
