using EnemyUI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RedEffect : PotionEffect
{
    private float tickTime = 0.5f;

    List<EnemyBT> enemies = new List<EnemyBT>();

    public override void OnEnable()
    {
        base.OnEnable();
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

    public IEnumerator TimerCoroutine(float duringTime)
    {
        float elapsedTime = 0f;
        float elapsedTickTime = 0f;

        EffectGenerator.Instance.ThrowPotion(transform.position, THROWTIME);
        yield return new WaitForSeconds(THROWTIME);

        ShowEffect();

        while (elapsedTime < duringTime)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime - elapsedTickTime > tickTime)
            {
                // µ•πÃ¡ˆ∏¶ ¡‹
                elapsedTickTime += tickTime;

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

    public override void StartEffect(float duringTime)
    {
        StartCoroutine(TimerCoroutine(duringTime));
    }

}
