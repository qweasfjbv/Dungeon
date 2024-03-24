using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionCard : CardBase
{
    [SerializeField]
    private GameObject effectPrefab;

    public override void ActivateEffect(Vector3 pos)
    {
        pos.z = 0;
        Instantiate(effectPrefab,pos, Quaternion.identity);
    }
}
