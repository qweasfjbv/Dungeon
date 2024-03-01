using Delaunay;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    // �� �Ѱ��� �ش��ϴ� GO
    [SerializeField] private GameObject GridPrefab;

    const int PIXEL = 1;
    const int ROOMCNT = 60;
    int minRoomSize = 1;
    int maxRoomSize = 8;

    int hallwayId = 200;

    private List<GameObject> rooms = new List<GameObject>();


    public Material lineMaterial;

    private Camera mainCamera;
    private HashSet<Vertex> points;
    private HashSet<GameObject> lines;

    private float meanHeight = 0;
    private float meanWidth = 0;

    private int[,] map; // 2���� �迭 ��

    int minX = int.MaxValue, minY = int.MaxValue;
    int maxX = int.MinValue, maxY = int.MinValue;

    void Awake()
    {
        mainCamera = Camera.main;
        points = new HashSet<Vertex>();
        lines = new HashSet<GameObject>();
    }

    private void Start()
    {
        StartCoroutine(TestCoroutine());
    }



    // �� �ȿ��� ���� ����Ʈ �����
    private Vector3 GetRandomPointInCircle(int rad)
    {
        float t = 2 * Mathf.PI * Random.Range(0f, 1f);
        float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
        float r = 0;

        if (u > 1) r = 2 - u;
        else r = u;

        return new Vector3(RoundPos(rad * r * Mathf.Cos(t), PIXEL), RoundPos(rad * r * Mathf.Sin(t), PIXEL), 0);
    }

    private Vector3 GetRandomScale(Vector3 pos)
    {
        return new Vector3(Random.Range(minRoomSize, maxRoomSize) * 2, Random.Range(minRoomSize, maxRoomSize) * 2, 1);
    }

    private int RoundPos(float n, int m)
    {
        return Mathf.FloorToInt(((n + m - 1) / m)) * m;
    }

    
    private IEnumerator TestCoroutine()
    {

        while (true)
        {
            meanHeight = 0;
            meanWidth = 0;

            float totalHeight = 0;
            float totalWidth = 0;

            for (int i = 0; i < ROOMCNT; i++)
            {
                rooms.Add(Instantiate(GridPrefab, GetRandomPointInCircle(10), new Quaternion(0, 0, 0, 0)));
                rooms[i].transform.localScale = GetRandomScale(rooms[i].transform.position);

                totalHeight += rooms[i].transform.localScale.y;
                totalWidth += rooms[i].transform.localScale.x;

                yield return new WaitForSeconds(0.03f);
            }

            meanHeight = totalHeight / ROOMCNT;
            meanWidth = totalWidth / ROOMCNT;

            for (int i = 0; i < ROOMCNT; i++)
            {
                rooms[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                rooms[i].GetComponent<Rigidbody2D>().gravityScale = 0f;
            }

            yield return new WaitForSeconds(5f);

            for (int i = 0; i < ROOMCNT; i++)
            {
                rooms[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                rooms[i].transform.position = new Vector3(RoundPos(rooms[i].transform.position.x, PIXEL), RoundPos(rooms[i].transform.position.y, PIXEL), 1);
                if (rooms[i].transform.localScale.x >= 1.25 * meanWidth && rooms[i].transform.localScale.y >= 1.25 * meanHeight)
                {
                    rooms[i].GetComponent<SpriteRenderer>().color = Color.black;
                    points.Add(new Vertex((int)rooms[i].transform.position.x, (int)rooms[i].transform.position.y));
                }
                else
                {
                    rooms[i].SetActive(false);
                }

            }
            if (points.Count >= 5) break;
            else
            {
                points.Clear();

                meanHeight = 0;
                meanWidth = 0;

                for (int i = 0; i < ROOMCNT; i++)
                {
                    Destroy(rooms[i].gameObject);
                }
                rooms.Clear();

            }
            yield return null;
        }

        GenerateMapArr();

        RegenerateLines();

        yield return null;

        CellularAutomata();
    }



    private void RegenerateLines()
    {
        foreach (var line in lines)
            Destroy(line);
        lines.Clear();

        var triangles = BowyerWatson.Triangulate(points);

        var graph = new HashSet<Delaunay.Edge>();
        foreach (var triangle in triangles)
            graph.UnionWith(triangle.edges);

        var tree = Kruskal.MinimumSpanningTree(graph);


        GenerateCorridors(tree);
    }

    private void GenerateHallways()
    {

    }
    void GenerateMapArr()
    {
        // �迭 ũ�� ������ ���� �ּ�/�ִ� ��ǥ �ʱ�ȭ

        // �ּ�/�ִ� ��ǥ Ž��
        foreach (var room in rooms)
        {
            Vector3 pos = room.transform.position;
            Vector3 scale = room.transform.localScale;

            minX = Mathf.Min(minX, Mathf.FloorToInt(pos.x-scale.x));
            minY = Mathf.Min(minY, Mathf.FloorToInt(pos.y - scale.y));
            maxX = Mathf.Max(maxX, Mathf.CeilToInt(pos.x + scale.x));
            maxY = Mathf.Max(maxY, Mathf.CeilToInt(pos.y + scale.y));
        }

        // �迭 ũ�� ��� �� �ʱ�ȭ
        int width = maxX - minX;
        int height = maxY - minY;
        map = new int[height, width];

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++) map[i, j] = -1;

        // �迭�� GameObject ����
        for (int i = 0; i < rooms.Count; i++)
        {
            Vector3 pos = rooms[i].transform.position;
            Vector3 scale = rooms[i].transform.localScale;

            // GameObject�� ũ��� ��ġ�� ����Ͽ� �迭�� ����
            for (int x = (int)-scale.x/2; x < scale.x/2; x++)
            {
                for (int y = (int)-scale.y/2; y < scale.y/2; y++)
                {
                    int mapX = Mathf.FloorToInt(pos.x - minX + x);
                    int mapY = Mathf.FloorToInt(pos.y - minY + y);
                    map[mapY, mapX] = i;
                }
            }
        }
    }


    void GenerateCorridors(IEnumerable<Delaunay.Edge> tree)
    {
        foreach (Delaunay.Edge edge in tree)
        {
            Vertex start = edge.a;
            Vertex end = edge.b;

            // �������� ���� ������ ���� ����
            CreateCorridor(start, end);
        }

        foreach (Delaunay.Edge edge in tree)
        {
            Vertex start = edge.a;
            Vertex end = edge.b;

            // �������� ���� ������ ���� ����
            CreateHallway(start, end);
        }


    }
    void CreateCorridor(Vertex start, Vertex end)
    {
        // ���� �Ǵ� ���� ���� ����
        if (start.x == end.x || start.y == end.y)
        {
            for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
            {
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    InstantiateGrid(x, y);
                }
            }
        }
        else // '��'�� �Ǵ� '��'�� ���� ����
        {
            // ��ü Vertex�� �߰��� ��� (���⼭�� �ܼ�ȭ�� ���� ���� �߽��� ���)
            int mapCenterX = map.GetLength(0) / 2;
            int mapCenterY = map.GetLength(1) / 2;

            // start�� end ������ �߰��� ���
            int midX = (start.x + end.x) / 2;
            int midY = (start.y + end.y) / 2;

            // �߰����� ��ü ���� �߽����� ���� ��� ��и鿡 ��ġ�ϴ��� �ľ�
            int quadrant = DetermineQuadrant(midX - mapCenterX - minX, midY - mapCenterY - minY);

            // ��и�� start�� end�� ����� ��ġ�� ���� '��' Ȥ�� '��' ���� ����
            if (quadrant == 2 || quadrant == 3)
            {
                // ��и��� 1 �Ǵ� 3�� ���
                CreateStraightPath(start.x, start.y,end.x, start.y); // ���η� ���� �̵�
                CreateStraightPath(end.x, start.y, end.x, end.y); // ���� ���η� �̵�
            }
            else if (quadrant == 1 || quadrant == 4)
            {
                // ��и��� 2 �Ǵ� 4�� ���
                CreateStraightPath(start.x, start.y,start.x, end.y); // ���η� ���� �̵�
                CreateStraightPath(start.x, end.y, end.x, end.y); // ���� ���η� �̵�
            }
        }
    }
    int DetermineQuadrant(int x, int y)
    {
        if (x >= 0 && y >= 0) return 1;
        if (x < 0 && y >= 0) return 2;
        if (x < 0 && y < 0) return 3;
        if (x >= 0 && y < 0) return 4;
        return 0; // �� ���� �߻����� ����
    }
    void CreateStraightPath(int startX, int startY, int endX, int endY)
    {
        for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
        {
            for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++)
            {
                InstantiateGrid(x, y);
            }
        }
    }

    void CreateStraightHall(int startX, int startY, int endX, int endY)
    {

        for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
        {
            for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++)
            {
                AddHallwayWidth(x, y);
            }
        }
    }

    void CreateHallway(Vertex start, Vertex end)
    {// ���� �Ǵ� ���� ���� ����
        if (start.x == end.x || start.y == end.y)
        {
            for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
            {
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    AddHallwayWidth(x, y);
                }
            }
        }
        else // '��'�� �Ǵ� '��'�� ���� ����
        {
            // ��ü Vertex�� �߰��� ��� (���⼭�� �ܼ�ȭ�� ���� ���� �߽��� ���)
            int mapCenterX = map.GetLength(0) / 2;
            int mapCenterY = map.GetLength(1) / 2;

            // start�� end ������ �߰��� ���
            int midX = (start.x + end.x) / 2;
            int midY = (start.y + end.y) / 2;

            // �߰����� ��ü ���� �߽����� ���� ��� ��и鿡 ��ġ�ϴ��� �ľ�
            int quadrant = DetermineQuadrant(midX - mapCenterX - minX, midY - mapCenterY - minY);

            // ��и�� start�� end�� ����� ��ġ�� ���� '��' Ȥ�� '��' ���� ����
            if (quadrant == 2 || quadrant == 3)
            {
                // ��и��� 1 �Ǵ� 3�� ���
                CreateStraightHall(start.x, start.y, end.x, start.y); // ���η� ���� �̵�
                CreateStraightHall(end.x, start.y, end.x, end.y); // ���� ���η� �̵�
            }
            else if (quadrant == 1 || quadrant == 4)
            {
                // ��и��� 2 �Ǵ� 4�� ���
                CreateStraightHall(start.x, start.y, start.x, end.y); // ���η� ���� �̵�
                CreateStraightHall(start.x, end.y, end.x, end.y); // ���� ���η� �̵�
            }
        }
    }

    private void InstantiateGrid(int x, int y)
    {
        if (map[y-minY, x - minX] == -1) // �ش� ��ġ�� �̹� �׸��尡 ���� ��쿡�� ����
        {
            GameObject grid = Instantiate(GridPrefab, new Vector3(x +0.5f, y + 0.5f, 0), Quaternion.identity);
            grid.GetComponent<SpriteRenderer>().color = Color.black;
            map[y - minY, x - minX] = hallwayId;
        }
        else if(map[y - minY, x - minX] != hallwayId)
        {
            if(rooms[map[y - minY, x - minX]].activeSelf)
            {

            }
            else
            {
                rooms[map[y - minY, x - minX]].SetActive(true);
                rooms[map[y - minY, x - minX]].GetComponent<SpriteRenderer>().color = Color.black;
            }
        }


    }

    private void AddHallwayWidth(int x, int y)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                var px = x + i; var py = y + j;
                if (px < minX || py < minY || py >= maxY || px >= maxX) continue;
                if (map[py - minY, px - minX] == hallwayId) continue;


                if (map[py - minY, px - minX] == -1 || !rooms[map[py - minY, px - minX]].activeSelf)
                {
                    map[py - minY, px - minX] = hallwayId;
                    GameObject grid = Instantiate(GridPrefab, new Vector3(px + 0.5f, py + 0.5f, 0), Quaternion.identity);
                    grid.GetComponent<SpriteRenderer>().color = Color.black;
                }
            }
        }
    }

    private void CellularAutomata()
    {
        for (int x = 0; x < maxX-minX; x++)
        {
            for (int y = 0; y < maxY-minY; y++)
            {
                if (map[y, x] != -1 || (map[y, x] != -1 && map[x, y] != hallwayId && rooms[map[x, y]].activeSelf)) continue;


                int nonWallCount = 0; // ���� �ƴ� ������ ���� ī��Ʈ

                // ���� ������ ���� 9ĭ �˻�
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        int checkX = x + offsetX;
                        int checkY = y + offsetY;

                        // �迭 ������ ����� ������ ����
                        if (checkX < 0 || checkX >= maxX - minX || checkY < 0 || checkY >= maxY - minY)
                        {
                            continue;
                        }
                        else if (map[checkY, checkX] != -1 && (map[checkY, checkX] != hallwayId && rooms[map[checkY, checkX]].activeSelf)) // ���� �ƴ� ���� Ȯ��
                        {
                            nonWallCount++;
                        }
                    }
                }

                // �ֺ��� ���� �ƴ� ������ 5ĭ �̻��̸� �Ϲ� �������� ����
                if (nonWallCount >= 5)
                {
                    map[y, x] = hallwayId; // �迭�� hallwayId ����

                    GameObject grid = Instantiate(GridPrefab, new Vector3(x + minX + 0.5f, y +minY + 0.5f, 0), Quaternion.identity);
                    grid.GetComponent<SpriteRenderer>().color = Color.black;
                    map[y, x] = hallwayId;
                }
            }
        }
    }

}
