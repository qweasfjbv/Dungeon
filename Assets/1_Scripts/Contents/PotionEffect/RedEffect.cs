using EnemyUI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RedEffect : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float duringTime = 5f;
    private float tickTime = 0.5f;

    List<EnemyBT> enemies = new List<EnemyBT>();

    private void OnEnable()
    {
        foreach (Transform child in transform)
            if (child.GetComponent<TextMeshPro>() != null)
                textMesh = child.GetComponent<TextMeshPro>();


        StartCoroutine(TimerCoroutine());
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyBT>() != null)
        {
            enemies.Add(collision.GetComponent<EnemyBT>());
        }
    }

        private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyBT>() != null)
            enemies.Remove(collision.GetComponent<EnemyBT>());
    }

    public IEnumerator TimerCoroutine()
    {
        float elapsedTime = 0f;
        float elapsedTickTime = 0f;

        while (elapsedTime < duringTime)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime - elapsedTickTime > tickTime)
            {
                // µ•πÃ¡ˆ∏¶ ¡‹
                elapsedTickTime += tickTime;
                Debug.Log(enemies.Count);
                foreach (EnemyBT enemy in enemies)
                {
                    enemy.OnDamaged(1);
                }
            }
            textMesh.text = (duringTime - elapsedTime).ToString("0.0");
            yield return null;
        }

        Destroy(this.gameObject);
    }


}
