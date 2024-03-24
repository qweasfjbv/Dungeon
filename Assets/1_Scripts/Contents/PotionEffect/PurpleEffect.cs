using EnemyUI.BehaviorTree;
using System.Collections;
using UnityEngine;

public class PurpleEffect : MonoBehaviour
{
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
        yield return new WaitForSeconds(5f);
        Destroy(this.gameObject);
    }

}
