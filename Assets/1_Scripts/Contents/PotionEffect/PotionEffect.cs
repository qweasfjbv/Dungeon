using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class PotionEffect : MonoBehaviour
{
    protected TextMeshPro textMesh;
    protected const float THROWTIME = 0.7f;

    public virtual void OnEnable()
    {
        foreach (Transform child in transform)
            if (child.GetComponent<TextMeshPro>() != null)
                textMesh = child.GetComponent<TextMeshPro>();

    }

    protected void ShowEffect()
    {

        this.GetComponent<CircleCollider2D>().enabled = true;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<SpriteRenderer>() == null) child.gameObject.SetActive(true);
            else
            {
                child.gameObject.SetActive(true);
                var tmpColor = child.GetComponent<SpriteRenderer>().color;
                tmpColor.a = 1f;
                child.GetComponent<SpriteRenderer>().color = tmpColor;
            }
        }
    }

    public void PreviewizeEffect()
    {
        this.GetComponent<CircleCollider2D>().enabled = false;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<SpriteRenderer>() == null) child.gameObject.SetActive(false);
            else
            {
                var tmpColor = child.GetComponent<SpriteRenderer>().color;
                tmpColor.a = 0.4f;
                child.GetComponent<SpriteRenderer>().color = tmpColor;
            }
        }
    }


    public abstract void StartEffect(float duringTime);

}
