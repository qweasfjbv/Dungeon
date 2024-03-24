using EnemyUI.BehaviorTree;
using System.Collections;
using TMPro;
using UnityEngine;

public class PurpleEffect : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float duringTime = 5f;

    private void OnEnable()
    {
        foreach (Transform child in transform)
            if (child.GetComponent<TextMeshPro>() != null) 
                textMesh = child.GetComponent<TextMeshPro>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyBT>() != null)
            collision.GetComponent<EnemyBT>().MoveDebuff(2);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyBT>() != null)
            collision.GetComponent<EnemyBT>().MoveBuff(2);
    }

    private void Start()
    {
        StartCoroutine(TimerCoroutine());
    }

    public IEnumerator TimerCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duringTime)
        {
            elapsedTime += Time.deltaTime;
            textMesh.text = (duringTime - elapsedTime).ToString("0.0");
            yield return null;
        }

        Destroy(this.gameObject);
    }

}
