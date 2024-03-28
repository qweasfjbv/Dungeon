using EnemyUI.BehaviorTree;
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
        float elapsedTickTime = -tickTime / 2;

        yield return new WaitForSeconds(PotionCard.THROWTIME);

        ShowEffect();

        while (elapsedTime < duringTime)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime - elapsedTickTime > tickTime)
            {
                // 데미지를 줌
                elapsedTickTime += tickTime;

                foreach (EnemyBT enemy in enemies)
                {
                    // TODO : 몬스터 구현하고 몬스터Recover로 변경
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
