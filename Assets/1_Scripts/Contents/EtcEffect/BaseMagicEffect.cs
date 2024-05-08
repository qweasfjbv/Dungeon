using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMagicEffect : MonoBehaviour
{

    // have to be set
    protected  float damage;
    protected  string tagName;

    protected bool isSet = false;

    public void SetDamage(float damage, string tagName)
    {
        this.damage = damage;
        this.tagName = tagName;

        isSet = true;
    }

}
