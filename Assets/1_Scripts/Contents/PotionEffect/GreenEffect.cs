using EnemyAI.BehaviorTree;
using System.Collections;
using UnityEngine;

public class GreenEffect : PotionEffect
{
    private float tickTime = 1f;


    public override void OnEnable()
    {
        base.OnEnable();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<BTree>() != null)
        {
            enemies.Add(collision.GetComponent<BTree>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<BTree>() != null)
            enemies.Remove(collision.GetComponent<BTree>());
    }

    public IEnumerator TimerCoroutine(float duringTime)
    {
        float elapsedTime = 0f;
        float elapsedTickTime = -tickTime / 2;

        yield return new WaitForSeconds(PotionCard.THROWTIME);

        ShowEffect();

        while (elapsedTime < duringTime)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime - elapsedTickTime > tickTime)
            {
                // µ•πÃ¡ˆ∏¶ ¡‹
                elapsedTickTime += tickTime;

                foreach (BTree enemy in enemies)
                {
                    if (enemy.CompareTag(Constants.TAG_MONSTER))
                        enemy.OnRecover(2);
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
