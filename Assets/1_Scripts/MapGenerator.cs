using Delaunay;
using EnemyUI.BehaviorTree;
using JPS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class MapGenerator: MonoBehaviour
{

    static MapGenerator s_instance;
    public static MapGenerator Instance { get { return s_instance; } }
    

    [Header("Map Generate Variables")]
    [SerializeField] private GameObject GridPrefab;
    [SerializeField] private int generateRoomCnt;
    [SerializeField] private int selectRoomCnt;
    [SerializeField] private int minRoomSize;
    [SerializeField] private int maxRoomSize;
    [SerializeField] private int smallMinRoomSize;
    [SerializeField] private int smallMaxRoomSize;
    [SerializeField] private int overlapOffset;

    private const int PIXEL = 1;
    private int hallwayId = 200;
    private int cellularId = 300;


    private List<GameObject> rooms = new List<GameObject>();
    private HashSet<Delaunay.Vertex> points = new HashSet<Delaunay.Vertex>();
    private HashSet<GameObject> lines = new HashSet<GameObject>();

    private List<Edge> hallwayEdges;

    private int[,] map; // 2���� �迭 ��


    private JumpPointSearch jpm;
    int minX = int.MaxValue, minY = int.MaxValue;
    int maxX = int.MinValue, maxY = int.MinValue;

    private Vector2Int startV;
    private Vector2Int endV;


    private List<(int index, Vector2 pos)> selectedRooms = new List<(int, Vector2)>();

    private void Awake()
    {

        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@MapGenerator");
            if (go == null)
            {
                go = new GameObject { name = "@MapGenerator" };
                go.AddComponent<MapGenerator>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<MapGenerator>();

        }
        else
        {
            Destroy(this.gameObject);
            return;
        }


    }

    private void Start()
    { 
        StartCoroutine(MapGenerateCoroutine());
    }


    int count = 0;
    bool again = false;
    

    #region PROCEDURE MAP GENERATE

    public Define.GridType GetGridType(int x, int y)
    {

        if(y < 0 || x < 0 || (maxX-minX)<= x || (maxY-minY)<= y) return Define.GridType.None;

        return (Define.GridType)map[y, x];
    }

    /*  
     *  �� ��������� ���� �ù��ϱ����� �ڷ�ƾ
     *  �������� �Ǵµ��� 3.5~5�ʴ� ��ٷ�����
     */
    private IEnumerator MapGenerateCoroutine()
    {
        // �� ��������
        for (int i = 0; i < generateRoomCnt; i++)
        {
            rooms.Add(Instantiate(GridPrefab, GetRandomPointInCircle(10), Quaternion.identity));
            if (i > selectRoomCnt)
            {
                rooms[i].transform.localScale = GetRandomScale(smallMinRoomSize, smallMaxRoomSize);
            }
            else
            {
                rooms[i].transform.localScale = GetRandomScale(minRoomSize, maxRoomSize);
            }
            yield return null;
        }

        // ���������� �ϱ� ���� Dynamic
        for (int i = 0; i < generateRoomCnt; i++)
        {
            rooms[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            rooms[i].GetComponent<Rigidbody2D>().gravityScale = 0f;
        }

        yield return new WaitForSeconds(5f);

        FindMainRooms(selectRoomCnt);

        GenerateMapArr();

        MainRoomFraming();
        ConnectRooms();
        GenerateHallways(hallwayEdges);
        //CellularAutomata(5);

        MapArrNormalization();
        AutoTiling();


        jpm = new JumpPointSearch(map);

        SelectEntrances();

    }

    private Vector3 GetRandomPointInCircle(int rad)
    {
        float t = 2 * Mathf.PI * Random.Range(0f, 1f);
        float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
        float r = 0;

        if (u > 1) r = 2 - u;
        else r = u;

        return new Vector3(RoundPos(rad * r * Mathf.Cos(t), PIXEL), RoundPos(rad * r * Mathf.Sin(t), PIXEL), 0);
    }
    private Vector3 GetRandomScale(int minS, int maxS)
    {
        int x = Random.Range(minS, maxS) * 2;
        int y = Random.Range(minS, maxS) * 2;


        return new Vector3(x, y, 1);
    }

    /// <summary>
    /// �������� �Ŀ� ��ġ�� ������ ��ȯ�ϱ� ���� ���
    /// </summary>
    /// <param name="n">��ȯ�� ��</param>
    /// <param name="m">�׸��� ���� (2�̸� return���� ¦���� ����)</param>
    /// <returns></returns>
    private int RoundPos(float n, int m)
    {
        return Mathf.FloorToInt(((n + m - 1) / m)) * m;
    }
    private void FindMainRooms(int roomCount)
    {
        // �� ���� ũ��, ����, �ε����� ������ ����Ʈ ����
        List<(float size, int index)> tmpRooms = new List<(float size, int index)>();

        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            rooms[i].GetComponent<BoxCollider2D>().isTrigger = true;
            rooms[i].transform.position = new Vector3(RoundPos(rooms[i].transform.position.x, PIXEL), RoundPos(rooms[i].transform.position.y, PIXEL), 1);


            Vector3 scale = rooms[i].transform.localScale;
            float size = scale.x * scale.y; // ���� ũ��(����) ���
            float ratio = scale.x / scale.y; // ���� ���� ���
            if (ratio > 2f || ratio < 0.5f) continue; // 1:2 �Ǵ� 2:1 ������ �ʰ��ϴ� ��� �ǳʶٱ�
            tmpRooms.Add((size, i));
        }

        // ���� ũ�⿡ ���� ������������ ����
        var sortedRooms = tmpRooms.OrderByDescending(room => room.size).ToList();

        // ��� ���� �ϴ� ��Ȱ��ȭ
        foreach (var room in rooms)
        {
            room.SetActive(false);
        }

        // ���� ������ �����ϴ� �� ���� �� ó��
        int count = 0;
        selectedRooms = new List<(int, Vector2)>();
        foreach (var roomInfo in sortedRooms)
        {
            if (count >= roomCount) break; // ���� �� ����
            GameObject room = rooms[roomInfo.index];
            SpriteRenderer renderer = room.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.red;
            }
            room.SetActive(true);
            points.Add(new Delaunay.Vertex((int)room.transform.position.x, (int)room.transform.position.y)); // points ����Ʈ�� �߰�
            selectedRooms.Add((roomInfo.index, new Vector2((int)room.transform.position.x, (int)room.transform.position.y)));
            count++;
        }

    }

    // �� ������ 2���� �迭�� ����
    private void GenerateMapArr()
    {
        // �迭 ũ�� ������ ���� �ּ�/�ִ� ��ǥ �ʱ�ȭ


        // �ּ�/�ִ� ��ǥ Ž��
        foreach (var room in rooms)
        {
            Vector3 pos = room.transform.position;
            Vector3 scale = room.transform.localScale;

            minX = Mathf.Min(minX, Mathf.FloorToInt(pos.x - scale.x));
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
            for (int x = (int)-scale.x / 2; x < scale.x / 2; x++)
            {
                for (int y = (int)-scale.y / 2; y < scale.y / 2; y++)
                {
                    int mapX = Mathf.FloorToInt(pos.x - minX + x);
                    int mapY = Mathf.FloorToInt(pos.y - minY + y);
                    map[mapY, mapX] = i;
                }
            }
        }
    }

    // ��γ� �ﰢ����, �ּ� ���д� Ʈ���� �� ����
    // + ���� ����
    private void ConnectRooms()
    {


        var triangles = DelaunayTriangulation.Triangulate(points);


        var graph = new HashSet<Delaunay.Edge>();
        foreach (var triangle in triangles)
            graph.UnionWith(triangle.edges);

        hallwayEdges = Kruskal.MinimumSpanningTree(graph);

    }

    private void GenerateHallways(IEnumerable<Delaunay.Edge> tree)
    {
        Vector2Int size1 = new Vector2Int(2, 2);
        Vector2Int size2 = new Vector2Int(2, 2);

        foreach (Delaunay.Edge edge in tree)
        {
            Delaunay.Vertex start = edge.a;
            Delaunay.Vertex end = edge.b;

            size1 = new Vector2Int((int)rooms[map[start.y-minY, start.x-minX]].transform.localScale.x, (int)rooms[map[start.y-minY, start.x-minX]].transform.localScale.y);
            size2 = new Vector2Int((int)rooms[map[end.y-minY, end.x-minX]].transform.localScale.x, (int)rooms[map[end.y-minY, end.x-minX]].transform.localScale.y);

            // �������� ���� ������ ���� ����
            CreateCorridor(start, end, size1, size2);
        }

        foreach (Delaunay.Edge edge in tree)
        {
            Delaunay.Vertex start = edge.a;
            Delaunay.Vertex end = edge.b;

            size1 = new Vector2Int((int)rooms[map[start.y - minY, start.x - minX]].transform.localScale.x, (int)rooms[map[start.y - minY, start.x - minX]].transform.localScale.y);
            size2 = new Vector2Int((int)rooms[map[end.y - minY, end.x - minX]].transform.localScale.x, (int)rooms[map[end.y - minY, end.x - minX]].transform.localScale.y);

            CreateCorridorWidth(start, end, size1, size2);
        }

    }

    private void CreateCorridor(Delaunay.Vertex start, Delaunay.Vertex end, Vector2Int startSize, Vector2Int endSize)
    {
        bool isHorizontalOverlap = Mathf.Abs(start.x - end.x) < ((startSize.x + endSize.x) / 2f - overlapOffset);
        bool isVerticalOverlap = Mathf.Abs(start.y - end.y) < ((startSize.y + endSize.y) / 2f - overlapOffset);

        // ���� ���� ����
        if (isVerticalOverlap)
        {
            int startY = Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y/2) + Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y/2);
            startY /= 2;
            for (int x = Mathf.Min(start.x + startSize.x/2, end.x + endSize.x/2) ; x <= Mathf.Max(start.x - startSize.x/2, end.x - endSize.x/2); x++)
            {
                InstantiateGrid(x, startY);
            }
        }
        // ���� ���� ����
        else if (isHorizontalOverlap)
        {
            int startX= Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2) + Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2);
            startX /= 2;
            for (int y = Mathf.Min(start.y + startSize.y/2, end.y + endSize.y/2); y <= Mathf.Max(start.y - startSize.y/2, end.y - endSize.y/2); y++)
            {
                InstantiateGrid(startX, y);
            }
        }
        else // '��'�� �Ǵ� '��'�� ���� ����
        {
            // ��ü Delaunay.Vertex�� �߰��� ���
            int mapCenterX = map.GetLength(0) / 2;
            int mapCenterY = map.GetLength(1) / 2;

            // start�� end ������ �߰��� ���
            int midX = (start.x + end.x) / 2;
            int midY = (start.y + end.y) / 2;

            // �߰����� ��ü ���� �߽����� ���� ��� ��и鿡 ��ġ�ϴ��� �ľ�
            int quadrant = DetermineQuadrant(midX - mapCenterX - minX, midY - mapCenterY - minY);

            // ��и�� start�� end�� ����� ��ġ�� ���� '��' Ȥ�� '��' ���� ����, �׻� start�� x�� ����.
            if (quadrant == 2 || quadrant == 3)
            {
                // ��и��� 2 �Ǵ� 3�� ���
                CreateStraightPath(start.x, start.y,end.x, start.y); // ���η� ���� �̵�
                CreateStraightPath(end.x, start.y, end.x, end.y); // ���� ���η� �̵�
            }
            else if (quadrant == 1 || quadrant == 4)
            {
                // ��и��� 1 �Ǵ� 4�� ���
                CreateStraightPath(start.x, start.y,start.x, end.y); // ���η� ���� �̵�
                CreateStraightPath(start.x, end.y, end.x, end.y); // ���� ���η� �̵�
            }
        }
    }
    private void CreateCorridorWidth(Delaunay.Vertex start, Delaunay.Vertex end, Vector2Int startSize, Vector2Int endSize)
    {
        bool isHorizontalOverlap = Mathf.Abs(start.x - end.x) < ((startSize.x + endSize.x) / 2f - overlapOffset);
        bool isVerticalOverlap = Mathf.Abs(start.y - end.y) < ((startSize.y + endSize.y) / 2f - overlapOffset);

        // ���� ���� ����
        if (isVerticalOverlap)
        {

            int startY = Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2) + Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2);
            startY /= 2;
            for (int x = (int)Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2); x <= (int)Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2); x++)
            {
                AddHallwayWidth(x, startY);
            }
        }
        // ���� ���� ����
        else if (isHorizontalOverlap)
        {
            int startX = Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2) + Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2);
            startX /= 2;
            for (int y = (int)Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2); y <= (int)Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2); y++)
            {
                AddHallwayWidth(startX, y);
            }
        }
        else // '��'�� �Ǵ� '��'�� ���� ����
        {
            // ��ü Delaunay.Vertex�� �߰��� ��� (���⼭�� �ܼ�ȭ�� ���� ���� �߽��� ���)
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
    private void CreateStraightPath(int startX, int startY, int endX, int endY)
    {
        for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
        {
            for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++)
            {
                InstantiateGrid(x, y);
            }
        }
    }
    private void CreateStraightHall(int startX, int startY, int endX, int endY)
    {

        for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
        {
            for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++)
            {
                AddHallwayWidth(x, y);
            }
        }
    }
    private int DetermineQuadrant(int x, int y)
    {
        if (x >= 0 && y >= 0) return 1;
        if (x < 0 && y >= 0) return 2;
        if (x < 0 && y < 0) return 3;
        if (x >= 0 && y < 0) return 4;
        return 0; // �� ���� �߻����� ����
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
                    //GameObject grid = Instantiate(GridPrefab, new Vector3(px + 0.5f, py + 0.5f, 0), Quaternion.identity);
                    //grid.GetComponent<SpriteRenderer>().color = Color.black;
                }
            }
        }
    }
    private void CellularAutomata(int n)
    {
        for (int x = 0; x < maxX - minX; x++)
        {
            for (int y = 0; y < maxY - minY; y++)
            {
                if (map[y, x] == hallwayId) continue;
                if ((map[y, x] != -1 && map[y, x] != hallwayId && rooms[map[y, x]].activeSelf)) continue;


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
                        else if (map[checkY, checkX] == -1) continue;
                        else if (map[checkY, checkX] == cellularId) continue;
                        else if (map[checkY, checkX] == hallwayId || rooms[map[checkY, checkX]].activeSelf) // ���� �ƴ� ���� Ȯ��
                        {
                            nonWallCount++;
                        }
                    }
                }

                //
                if (nonWallCount >= n)
                {
                    //grid.GetComponent<SpriteRenderer>().color = Color.black;
                    map[y, x] = cellularId;
                }
            }
        }


        for (int x = 0; x < maxX - minX; x++)
        {
            for (int y = 0; y < maxY - minY; y++)
            {
                if (map[y, x] == cellularId) map[y, x] = hallwayId;
            }
        }
    }
    private void InstantiateGrid(int x, int y)
    {
        if (map[y - minY, x - minX] == -1) // �ش� ��ġ�� �̹� �׸��尡 ���� ��쿡�� ����
        {
            //GameObject grid = Instantiate(GridPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
            //grid.GetComponent<SpriteRenderer>().color = Color.black;
            map[y - minY, x - minX] = hallwayId;
        }
        else if (map[y - minY, x - minX] != hallwayId)
        {
            if (rooms[map[y - minY, x - minX]].activeSelf)
            {

            }
            else
            {
                rooms[map[y - minY, x - minX]].SetActive(true);
                rooms[map[y - minY, x - minX]].GetComponent<SpriteRenderer>().color = Color.black;
            }
        }


    }

    private void MainRoomFraming()
    {
        foreach (var (index, pos) in selectedRooms)
        {
            int selectedId = index;

            rooms[selectedId].transform.position = pos - new Vector2(minX, minY) + new Vector2(0, 0.5f);
            rooms[selectedId].transform.localScale = rooms[selectedId].transform.localScale + new Vector3(0, 1f, 0);

            rooms[selectedId].GetComponent<SpriteRenderer>().color = Color.clear;
            rooms[selectedId].GetComponent<SpriteRenderer>().sortingOrder = 4;
            //rooms[selectedId].AddComponent<RoomOnMouseOver>();
            
            // ���簢�� ������ �ּ� �� �ִ� x, y ��ǥ�� ã���ϴ�.
            int minIx = int.MaxValue, minIy = int.MaxValue;
            int maxIx = int.MinValue, maxIy = int.MinValue;

            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if (map[y, x] == selectedId)
                    {
                        minIx = Mathf.Min(minIx, x);
                        maxIx = Mathf.Max(maxIx, x);
                        minIy = Mathf.Min(minIy, y);
                        maxIy = Mathf.Max(maxIy, y);
                    }
                }
            }
            // ã�� ��踦 ���� ���κ��� 0���� �����մϴ�.
            for (int y = minIy; y <= maxIy; y++)
            {
                for (int x = minIx; x <= maxIx; x++)
                {
                    // �����ڸ����� Ȯ���ϰ�, �����ڸ���� 0���� ����
                    if (x == minIx || x == maxIx || y == minIy || y == maxIy)
                    {
                        map[y, x] = -1;
                    }
                }
            }
        }
    }


    #endregion

    #region AUTO TILING

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap cliffTilemap;
    [SerializeField] private Tilemap shadowTilemap;
    [SerializeField] private Tilemap colliderTilemap;

    [Header("Tiles")]
    [SerializeField] private Tile wall_Top_Left;
    [SerializeField] private Tile wall_Top_Right;
    [SerializeField] private Tile wall_Top_Center;
    [SerializeField] private Tile wall_Bottom_Left;
    [SerializeField] private Tile wall_Bottom_Right;
    [SerializeField] private Tile wall_Bottom;
    [SerializeField] private Tile wall_Top;
    [SerializeField] private Tile wall_Right;
    [SerializeField] private Tile wall_Left;
    [SerializeField] private Tile wall_Center;
    [SerializeField] private Tile wall_Center_Center;
    [SerializeField] private Tile wall_Center_Right;
    [SerializeField] private Tile wall_Center_Left;
    [SerializeField] private Tile floor;
    [SerializeField] private Tile cliff_0;
    [SerializeField] private Tile cliff_1;
    [SerializeField] private Tile wall_T;

    [Header("Random Tiles")]
    [SerializeField] private Tile wall_Top_Random;
    [SerializeField] private Tile floor_Random_0;
    [SerializeField] private Tile floor_Random_1;
    [SerializeField] private Tile floor_Random_2;
    [SerializeField] private Tile floor_Random_3;

    [Header("Shadow Tiles")]
    [SerializeField] private Tile shadow_Right_Top;
    [SerializeField] private Tile shadow_Right;
    [SerializeField] private Tile shadow_Right_Bottom;

    [SerializeField] private Tile black_Tile;

    #region bitmasks

    const int TopMask = (1 << 1) | (1 << 3) | (1 << 5);
    const int BottomMask = (1 << 3) | (1 << 5) | (1 << 7);
    const int LeftMask = (1 << 1) | (1 << 3) | (1 << 7);
    const int RightMask = (1 << 1) | (1 << 5) | (1 << 7);

    const int TopLeftMask_0 = (1 << 3) | (1 << 1) | (1 << 0);
    const int TopRightMask_0 = (1 << 1) | (1 << 2) | (1 << 5);
    const int BottomLeftMask_0 = (1 << 3) | (1 << 6) | (1 << 7);
    const int BottomRightMask_0 = (1 << 5) | (1 << 7) | (1 << 8);
    const int TopLeftMask_1 = (1 << 5) | (1 << 7);
    const int TopRightMask_1 = (1 << 3) | (1 << 7);
    const int BottomLeftMask_1 = (1 << 1) | (1 << 5);
    const int BottomRightMask_1 = (1 << 1) | (1 << 3);


    const int TopMatch = 1 << 1;
    const int BottomMatch = 1 << 7;
    const int LeftMatch = 1 << 3;
    const int RightMatch = 1 << 5;

    const int TopLeftMatch_0 = 1 << 0;
    const int TopRightMatch_0 = 1 << 2;
    const int BottomLeftMatch_0 = 1 << 6;
    const int BottomRightMatch_0 = 1 << 8;
    const int TopLeftMatch_1 = (1 << 5) | (1 << 7);
    const int TopRightMatch_1 = (1 << 3) | (1 << 7);
    const int BottomLeftMatch_1 = (1 << 1) | (1 << 5);
    const int BottomRightMatch_1 = (1 << 1) | (1 << 3);


    const int ExceptionMask = (1 << 1) | (1 << 3) | (1 << 5) | (1 << 7);
    const int ExceptionMask_0 = (1 << 1) | (1 << 5) | (1 << 7);
    const int ExceptionMask_1 = (1 << 1) | (1 << 3) | (1 << 7);
    const int ExceptionMask_2 = (1 << 1) | (1 << 3) | (1 << 5);
    const int ExceptionMask_3 = (1 << 3) | (1 << 5) | (1 << 7);

    const int ExceptionMatch = (1 << 1) | (1 << 3) | (1 << 5) | (1 << 7);
    const int ExceptionMatch_0 = (1 << 1) | (1 << 5) | (1 << 7);
    const int ExceptionMatch_1 = (1 << 1) | (1 << 3) | (1 << 7);
    const int ExceptionMatch_2 = (1 << 1) | (1 << 3) | (1 << 5);
    const int ExceptionMatch_3 = (1 << 3) | (1 << 5) | (1 << 7);

    const int ExceptionMask_T1 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 5) | (1 << 4) | (1 << 7);
    const int ExceptionMask_T2 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 5) | (1 << 4) | (1 << 7);
    const int ExceptionMask_T3 = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 5) | (1 << 4);

    const int ExceptionMatch_T1 = (1 << 0) | (1<<7);
    const int ExceptionMatch_T2 = (1 << 2) | (1 << 7);
    const int ExceptionMatch_T3 = (1 << 0) | (1 << 2);


    const int ShadowMask = (1 << 0) | (1 << 3) | (1 << 6);

    const int ShadowTopMatch = (1 << 0) | (1 << 3);
    const int ShadowMidMatch = (1 << 0) | (1 << 3) | (1 << 6);
    const int ShadowBottomMatch = (1 << 3) | (1 << 6);

    #endregion

    private void MapArrNormalization()
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] < 0 || (map[i, j] != hallwayId && !rooms[map[i, j]].activeSelf)) { 

                    map[i, j] = (int)Define.GridType.None;
                    colliderTilemap.SetTile(new Vector3Int(j, i, 0), floor);
                }
                else if (map[i, j] == hallwayId) map[i, j] = (int)Define.GridType.HallWay;
                else map[i, j] = (int)Define.GridType.MainRoom;
            }
        }
    }

    // Floor->Wall->Exception->Cliff->Shadow
    private void AutoTiling()
    {
        for (int i = map.GetLength(0)-1; i>=0; i--)
        {
            for (int j = map.GetLength(1)-1; j >= 0 ; j--)
            {
                if (map[i, j] == (int)Define.GridType.None)
                {
                    PlaceTile(j, i, 2);
                }
                else
                {
                    PlaceTile(j, i, 1);
                }
            }

        }

        for (int i = map.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = map.GetLength(1) - 1; j >= 0; j--)
            {
                PlaceExceptionTiles(j, i);
            }
        }

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {

                PlaceShadowTile(j, i);
                PlaceCliffTile(j, i);
            }
        }

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                PlaceBlackTile(j, i);
            }
        }


    }

    // tileType : 1�̸� �ٴ�, 2�� ��
    private void PlaceTile(int x, int y, int tileType)
    {
        Tile tile = null;
        Vector3Int tilePos = new Vector3Int(x, y, 0);

        switch (tileType)
        {
            case 1: // floor
                tile = floor;
                floorTilemap.SetTile(tilePos, GetRandomFloorTile());
                break;
            case 2: // wall
                tile = DetermineWallTile(x, y);
                if (tile == null) break;

                if (tile == wall_Left || tile == wall_Right)
                {
                    wallTilemap.SetTile(tilePos, tile);
                }
                else if(tile == wall_Top_Left || tile == wall_Top_Right)
                {
                    wallTilemap.SetTile(tilePos + new Vector3Int(0, 1, 0), tile);
                    if (tile == wall_Top_Left) wallTilemap.SetTile(tilePos, wall_Left);
                    else wallTilemap.SetTile(tilePos, wall_Right);
                }
                else if(tile == wall_Top_Center)
                {
                    wallTilemap.SetTile(tilePos + new Vector3Int(0, 1, 0), tile);
                    wallTilemap.SetTile(tilePos, wall_Center_Center);
                }
                else
                {
                    wallTilemap.SetTile(tilePos + new Vector3Int(0, 1, 0), tile);

                    if (tile == wall_Bottom_Left || tile == wall_Bottom_Right || tile == wall_Bottom)
                    {
                        if (tile == wall_Bottom_Left)
                        {
                            wallTilemap.SetTile(tilePos, wall_Center_Left);
                        }
                        else if (tile == wall_Bottom_Right)
                        {
                            wallTilemap.SetTile(tilePos, wall_Center_Right);
                        }
                        else
                        {
                            wallTilemap.SetTile(tilePos, wall_Center);
                        }
                    }
                    else
                    {
                        wallTilemap.SetTile(tilePos, wall_Center);
                    }
                }
                break;
            default:
                break;
        }
    }

    private void PlaceShadowTile(int x, int y)
    {

        Vector3Int pos = new Vector3Int(x, y, 0);
        if (wallTilemap.GetTile(pos) != null) return;

        int pattern = CalculateShadowPattern(x, y);
        Tile shTile = null;


        if (Matches(pattern, ShadowMask, ShadowMidMatch)) shTile = shadow_Right;
        else if (Matches(pattern, ShadowMask, ShadowTopMatch)) shTile = shadow_Right_Top;
        else if (Matches(pattern, ShadowMask, ShadowBottomMatch)) shTile = shadow_Right_Bottom;

        shadowTilemap.SetTile(pos, shTile);

    }

    private void PlaceCliffTile(int x, int y)
    {
        Vector3Int tilePos = new Vector3Int(x, y, 0);

        Tile wallTile = wallTilemap.GetTile<Tile>(tilePos);

        if (wallTile == wall_Center_Left || wallTile == wall_Center_Right || wallTile == wall_Center)
        {
            if (wallTilemap.GetTile<Tile>(tilePos - new Vector3Int(0, 1, 0)) == null && floorTilemap.GetTile<Tile>(tilePos - new Vector3Int(0, 1, 0)) == null)
                cliffTilemap.SetTile(tilePos - new Vector3Int(0, 1, 0), cliff_0);
            if (wallTilemap.GetTile<Tile>(tilePos - new Vector3Int(0, 2, 0)) == null && floorTilemap.GetTile<Tile>(tilePos - new Vector3Int(0, 2, 0)) == null)
                cliffTilemap.SetTile(tilePos - new Vector3Int(0, 2, 0), cliff_1);
        }


    }

    private void PlaceBlackTile(int x, int y)
    {
        Vector3Int tilePos = new Vector3Int(x, y, 0);


        if (wallTilemap.GetTile<Tile>(tilePos) == null && floorTilemap.GetTile<Tile>(tilePos) == null && cliffTilemap.GetTile<Tile>(tilePos) == null)
        {
            wallTilemap.SetTile(tilePos, black_Tile);
        }
    }


    private void PlaceExceptionTiles(int x, int y)
    {
        Vector3Int tilePos = new Vector3Int(x, y, 0);
        int pattern = CalculatePattern(x, y);

        if (Matches(pattern, ExceptionMask_T1, ExceptionMatch_T1) || Matches(pattern, ExceptionMask_T2, ExceptionMatch_T2) || Matches(pattern, ExceptionMask_T3, ExceptionMatch_T3))
        {
            wallTilemap.SetTile(tilePos + new Vector3Int(0, 1, 0), wall_T);
            wallTilemap.SetTile(tilePos, wall_Right);
        }
    }

    private Tile DetermineWallTile(int x, int y)
    {
        // ���� ���
        int pattern = CalculatePattern(x, y);

        // ���Ͽ� ���� Ÿ�� ����


        if (Matches(pattern, ExceptionMask, ExceptionMatch)) return wall_Top_Center;
        if (Matches(pattern, ExceptionMask_0, ExceptionMatch_0)) return wall_Top_Center;
        if (Matches(pattern, ExceptionMask_1, ExceptionMatch_1)) return wall_Top_Center;
        if (Matches(pattern, ExceptionMask_2, ExceptionMatch_2)) return wall_Top_Center;

        if (Matches(pattern, ExceptionMask_3, ExceptionMatch_3)) {

            wallTilemap.SetTile(new Vector3Int(x, y, 0) + new Vector3Int(0, 1, 0), wall_Right);
            wallTilemap.SetTile(new Vector3Int(x, y, 0), wall_Right);
            return null;
                }


        if (Matches(pattern, TopMask, TopMatch)) return GetRandomTopTile();
        if (Matches(pattern, BottomMask, BottomMatch)) return wall_Bottom;
        if (Matches(pattern, LeftMask, LeftMatch)) return wall_Left;
        if (Matches(pattern, RightMask, RightMatch)) return wall_Right;
        if (Matches(pattern, TopLeftMask_0, TopLeftMatch_0)) return wall_Top_Left;
        if (Matches(pattern, TopRightMask_0, TopRightMatch_0)) return wall_Top_Right;
        if (Matches(pattern, BottomLeftMask_0, BottomLeftMatch_0)) return wall_Bottom_Left;
        if (Matches(pattern, BottomRightMask_0, BottomRightMatch_0)) return wall_Bottom_Right;

        if (Matches(pattern, TopLeftMask_1, TopLeftMatch_1)) return wall_Top_Left;
        if (Matches(pattern, TopRightMask_1, TopRightMatch_1)) return wall_Top_Right;
        if (Matches(pattern, BottomLeftMask_1, BottomLeftMatch_1)) return wall_Bottom_Left;
        if (Matches(pattern, BottomRightMask_1, BottomRightMatch_1)) return wall_Bottom_Right;

        // �⺻��
        return null;
    }

    private Tile GetRandomFloorTile()
    {
        int rInt = Random.Range(0, 200);
        switch (rInt) {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
                return floor_Random_0;
            case 11:
                return floor_Random_1;
            case 12:
                return floor_Random_2;
            case 13:
                return floor_Random_3;
            default:
                return floor;
        }

    }

    private Tile GetRandomTopTile()
    {
        if (Random.Range(0, 7) == 0) return wall_Top_Random;
        else return wall_Top;
    }

    int[] surrX = { 1, 0, -1, 1, 0, -1, 1, 0, -1 };
    int[] surrY = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
    int CalculatePattern(int x, int y)
    {
        int pattern = 0;
        int bitIndex = 0;

        for (int i = 0; i < surrX.Length; i++)
        {
            int checkX = x + surrX[i];
            int checkY = y + surrY[i];

            // �� ���� ������ �˻�
            if (checkX >= 0 && checkX < map.GetLength(1) && checkY >= 0 && checkY < map.GetLength(0))
            {
                if (map[checkY, checkX] != (int) Define.GridType.None)
                {
                    pattern |= (1 << bitIndex);
                }
            }

            bitIndex++;
        }

        return pattern;
    }
    int CalculateShadowPattern(int x, int y)
    {
        int pattern = 0;
        int bitIndex = 0;

        for (int i = 0; i < surrX.Length; i++)
        {
            int checkX = x + surrX[i];
            int checkY = y + surrY[i];

            // �� ���� ������ �˻�
            if (checkX >= 0 && checkX < map.GetLength(1) && checkY >= 0 && checkY < map.GetLength(0))
            {
                if (wallTilemap.GetTile(new Vector3Int(checkX, checkY, 0)) != null)
                {
                    pattern |= (1 << bitIndex);
                }
            }

            bitIndex++;
        }

        return pattern;
    }

    bool Matches(int pattern, int mask, int match)
    {
        return (pattern & mask) == match;
    }

    #endregion


    #region SELECT ENTRANCE

    [Header("ENTRACE")]
    [SerializeField] private GameObject floorEntrance;
    [SerializeField] private GameObject floorExit;

    private void SelectEntrances()
    {
        Vector2Int start = new Vector2Int(0, 0);
        Vector2Int end = new Vector2Int(0, 0);

        int maxDis = -1;
        foreach (var point1 in points)
        {
            foreach(var point2 in points)
            {
                if (point1 == point2) continue;
                var tmpDis = jpm.GetPathDistance(new Vector2Int(point1.y - minY, point1.x - minX), new Vector2Int(point2.y - minY, point2.x - minX));
                if (maxDis < tmpDis)
                {
                    maxDis = tmpDis;
                    start = new Vector2Int(point1.x - minX, point1.y - minY);
                    end = new Vector2Int(point2.x - minX, point2.y - minY);
                }


            }
        }

        // TODO : Instantiate���� ����

        floorEntrance.transform.position = new Vector3(start.x, start.y, 0);
        floorExit.transform.position = new Vector3(end.x, end.y, 0);
        
    }

    #endregion


    #region PATH FINDING

    [Header("DEBUG")]
    [SerializeField] GameObject Line;
    [SerializeField] Transform PathTest;
    public List<Vector2> PreprocessPath(Vector2Int startPoint, Vector2Int endPoint)
    {
        var pathList = (jpm.PathFind(startPoint, endPoint));

        List<Vector2> retList = new List<Vector2>();

        Vector2Int prevPoint = new Vector2Int(0, 0);
        foreach (var point in pathList)
        { 
            
            retList.Add(new Vector2(point.y + 0.5f, point.x + 0.5f));


            if (prevPoint.x != 0 || prevPoint.y != 0)
            {
                /*
                GameObject line = Instantiate(Line);
                line.GetComponent<LineRenderer>().SetPosition(0, new Vector3(prevPoint.y, prevPoint.x, -1));
                line.GetComponent<LineRenderer>().SetPosition(1, new Vector3(point.y, point.x, -1));
                line.GetComponent<LineRenderer>().startWidth = 0.3f;
                line.GetComponent<LineRenderer>().endWidth = 0.3f;
                */
            }

            prevPoint = point;
        }


        return retList;
    }

    public EnemyBT SummonEnemy()
    {
        var tmpEnemy = Instantiate(PathTest);
        tmpEnemy.transform.position = new Vector3(floorEntrance.transform.position.x, floorEntrance.transform.position.y, 0);
        tmpEnemy.GetComponent<EnemyBT>().SetValues(new Vector2Int(Mathf.FloorToInt(floorExit.transform.position.x), Mathf.FloorToInt(floorExit.transform.position.y)));
        tmpEnemy.gameObject.SetActive(true);

        return tmpEnemy.GetComponent<EnemyBT>();
    }

    #endregion
}
