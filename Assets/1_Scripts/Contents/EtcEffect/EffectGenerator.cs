using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectGenerator : MonoBehaviour
{


    static EffectGenerator s_instance;
    public static EffectGenerator Instance { get { return s_instance; } }

    [SerializeField] private GameObject imagePrefab;

    [SerializeField] private Sprite potionSprite;


    private void Awake()
    {

        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@EffectGenerator");
            if (go == null)
            {
                go = new GameObject { name = "@EffectGenerator" };
                go.AddComponent<EffectGenerator>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<EffectGenerator>();

        }
        else
        {
            Destroy(this.gameObject);
            return;
        }


    }

    public void ThrowPotion(Vector3 dest, float throwTime)
    {
        var tmpPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0, 0));
        var go = Instantiate(imagePrefab, tmpPos, Quaternion.identity);
        go.transform.localScale = go.transform.localScale * (5);

        go.GetComponent<SpriteRenderer>().sprite = potionSprite;
        go.AddComponent<ThrowEffect>();
        go.GetComponent<ThrowEffect>().Throw(tmpPos, dest, throwTime);
    }


}
