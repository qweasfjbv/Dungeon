using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectGenerator : MonoBehaviour
{


    static EffectGenerator s_instance;
    public static EffectGenerator Instance { get { return s_instance; } }

    [SerializeField] private GameObject imagePrefab;



    private void Awake()
    {
        s_instance = gameObject.GetComponent<EffectGenerator>();
    }

    public void ThrowPotion(Vector3 dest, float throwTime, Sprite potionSprite)
    {
        var tmpPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0, 0));
        var go = Instantiate(imagePrefab, tmpPos, Quaternion.identity);

        go.transform.localScale = go.transform.localScale * (2);

        go.GetComponent<SpriteRenderer>().sprite = potionSprite;
        go.GetComponent<ThrowEffect>().Throw(tmpPos, dest, throwTime);
    }

    public GameObject InstanceEffect(GameObject go, Vector3 pos, Quaternion quat)
    {
        var eff = Instantiate(go, pos, quat);
        return eff;
    }

}
