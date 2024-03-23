using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class TestCard : CardBase
{
    [SerializeField]
    private GameObject gridPrefab;

    public override void ActivateEffect(Vector3 pos)
    {
        float rad = 1f;
        int minX = Mathf.FloorToInt(pos.x - rad);
        int maxX = Mathf.CeilToInt(pos.x + rad);
        int minY = Mathf.FloorToInt(pos.y - rad);
        int maxY = Mathf.CeilToInt(pos.y + rad);

        var mapMG = GameObject.FindGameObjectWithTag("Map").GetComponent<MapGenerator>();

        // 주어진 반경 내의 모든 정수 좌표를 찾음
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector2Int point = new Vector2Int(x, y);
                if (mapMG.GetGridType(point.x, point.y) == Define.GridType.None) continue;

                Instantiate(gridPrefab, point + new Vector2(0.5f, 0.5f), Quaternion.identity);
            }
        }
    }
}
