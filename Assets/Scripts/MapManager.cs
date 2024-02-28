using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    // 방 한개에 해당하는 GO
    [SerializeField] private GameObject Unit;

    private List<GameObject> rooms = new List<GameObject>();
    const int PIXEL = 1;
    const int ROOMCNT = 50;

    private void Start()
    {
        StartCoroutine(TestCoroutine());
    }

    // 원 안에서 랜덤 포인트 만들기
    private Vector3 GetRandomPointInCircle(int rad)
    {
        float t = 2 * Mathf.PI * Random.Range(0f, 1f);
        float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
        float r = 0;

        if (u > 1) r = 2 - u;
        else r = u;

        return new Vector3(RoundPos(rad * r * Mathf.Cos(t), PIXEL), RoundPos(rad * r * Mathf.Sin(t), PIXEL), 0);
    }

    private Vector3 GetRandomScale()
    {
        return new Vector3(Random.Range(3, 10), Random.Range(3, 10), 1);
    }

    private int RoundPos(float n, int m)
    {
        return Mathf.FloorToInt(((n + m - 1) / m)) * m;
    }

    private IEnumerator TestCoroutine()
    {
        for (int i = 0; i < ROOMCNT; i++)
        {
            rooms.Add(Instantiate(Unit, GetRandomPointInCircle(10), new Quaternion(0, 0, 0, 0)));
            rooms[i].transform.localScale = GetRandomScale();
            yield return new WaitForSeconds(0.03f);
        }
        
        for(int i=0; i< ROOMCNT; i++)
        {
            rooms[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            rooms[i].GetComponent<Rigidbody2D>().gravityScale = 0f;
        }

        yield return new WaitForSeconds(3f);

        /*
        for (int i = 0; i < ROOMCNT; i++)
        {
            rooms[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            rooms[i].transform.position = new Vector3(RoundPos(rooms[i].transform.position.x, PIXEL), RoundPos(rooms[i].transform.position.y, PIXEL), 0);
        }
        */
    }



}
