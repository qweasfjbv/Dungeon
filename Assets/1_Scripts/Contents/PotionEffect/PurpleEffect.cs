using EnemyAI.BehaviorTree;
using System.Collections;
using TMPro;
using UnityEngine;

public class PurpleEffect : PotionEffect
{

    public override void OnEnable()
    {
        base.OnEnable();
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


    public IEnumerator TimerCoroutine(float duringTime)
    {
        yield return new WaitForSeconds(PotionCard.THROWTIME);
        ShowEffect();


        float elapsedTime = 0f;
        while (elapsedTime < duringTime)
        {
            elapsedTime += Time.deltaTime;
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
