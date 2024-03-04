using Delaunay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MapGenerator: MonoBehaviour
{
    [Header("Map Generate Variables")]
    [SerializeField] private GameObject GridPrefab;
    [SerializeField] private int generateRoomCnt = 60;
    [SerializeField] private int selectRoomCnt = 7;
    [SerializeField] private int minRoomSize = 2;
    [SerializeField] private int maxRoomSize = 8;
    [SerializeField] private int overlapOffset = 3;

    private const int PIXEL = 1;
    private int hallwayId = 200;
    
    private List<GameObject> rooms = new List<GameObject>();
    private HashSet<Delaunay.Vertex> points = new HashSet<Delaunay.Vertex>();
    private HashSet<GameObject> lines = new HashSet<GameObject>();

    private int[,] map; // 2���� �迭 ��

    private int minX = int.MaxValue, minY = int.MaxValue;
    private int maxX = int.MinValue, maxY = int.MinValue;



    private void Start()
    {
        StartCoroutine(MapGenerateCoroutine());
    }




    #region PROCEDURE MAP GENERATE

    /*  
     *  �� ��������� ���� �ù��ϱ����� �ڷ�ƾ
     *  �������� �Ǵµ��� 3.5~5�ʴ� ��ٷ�����
     */
    private IEnumerator MapGenerateCoroutine()
    {

        // �� ��������
        for (int i = 0; i < generateRoomCnt; i++)
        {
            rooms.Add(Instantiate(GridPrefab, GetRandomPointInCircle(10), new Quaternion(0, 0, 0, 0)));
            rooms[i].transform.localScale = GetRandomScale();

            yield return new WaitForSeconds(0.03f);
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

        RegenerateLines();

        CellularAutomata();

        MapArrNormalization();
        AutoTiling();
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
    private Vector3 GetRandomScale()
    {
        return new Vector3(Random.Range(minRoomSize, maxRoomSize) * 2, Random.Range(minRoomSize, maxRoomSize) * 2, 1);
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
        List<(float size, int index)> roomSizes = new List<(float size, int index)>();

        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            rooms[i].transform.position = new Vector3(RoundPos(rooms[i].transform.position.x, PIXEL), RoundPos(rooms[i].transform.position.y, PIXEL), 1);

            Vector3 scale = rooms[i].transform.localScale;
            float size = scale.x * scale.y; // ���� ũ��(����) ���
            float ratio = scale.x / scale.y; // ���� ���� ���
            if (ratio > 2f || ratio < 0.5f) continue; // 1:2 �Ǵ� 2:1 ������ �ʰ��ϴ� ��� �ǳʶٱ�
            roomSizes.Add((size, i)); // ũ��, ����, ���� �ε��� ����
        }

        // ���� ũ�⿡ ���� ������������ ����
        var sortedRooms = roomSizes.OrderByDescending(room => room.size).ToList();

        // ��� ���� �ϴ� ��Ȱ��ȭ
        foreach (var room in rooms)
        {
            room.SetActive(false);
        }

        // ���� ������ �����ϴ� �� ���� �� ó��
        int count = 0;
        foreach (var roomInfo in sortedRooms)
        {
            if (count >= roomCount) break; // ���� �� ����
            GameObject room = rooms[roomInfo.index];
            SpriteRenderer renderer = room.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.black;
            }
            room.SetActive(true);
            points.Add(new Delaunay.Vertex((int)room.transform.position.x, (int)room.transform.position.y)); // points ����Ʈ�� �߰�
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
    private void RegenerateLines()
    {
        foreach (var line in lines)
            Destroy(line);
        lines.Clear();

        var triangles = DelaunayTriangulation.Triangulate(points);

        var graph = new HashSet<Delaunay.Edge>();
        foreach (var triangle in triangles)
            graph.UnionWith(triangle.edges);

        var tree = Kruskal.MinimumSpanningTree(graph);

        GenerateHallways(tree);
    }
    private void GenerateHallways(IEnumerable<Delaunay.Edge> tree)
    {
        Vector2Int size1 = new Vector2Int(2, 2);
        Vector2Int size2 = new Vector2Int(2, 2);

        foreach (Delaunay.Edge edge in tree)
        {
            Delaunay.Vertex start = edge.a;
            Delaunay.Vertex end = edge.b;

            for (int i = 0; i < generateRoomCnt; i++)
            {
                var pos = rooms[i].transform.position;
                if (pos.x == start.x && pos.y == start.y)
                {
                    size1 = new Vector2Int((int)rooms[i].transform.localScale.x, (int)rooms[i].transform.localScale.y);
                }
                else if (pos.x == end.x && pos.y == end.y)
                {
                    size2 = new Vector2Int((int)rooms[i].transform.localScale.x, (int)rooms[i].transform.localScale.y);
                }
            }
            // �������� ���� ������ ���� ����
            CreateCorridor(start, end, size1, size2);
        }

        foreach (Delaunay.Edge edge in tree)
        {
            Delaunay.Vertex start = edge.a;
            Delaunay.Vertex end = edge.b;

            for (int i = 0; i < generateRoomCnt; i++)
            {
                var pos = rooms[i].transform.position;
                if (pos.x == start.x && pos.y == start.y)
                {
                    size1 = new Vector2Int((int)rooms[i].transform.localScale.x, (int)rooms[i].transform.localScale.y);
                }
                else if(pos.x == end.x && pos.y == end.y)
                {
                    size2 = new Vector2Int((int)rooms[i].transform.localScale.x, (int)rooms[i].transform.localScale.y);
                }
            }
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
            startY = startY / 2;
            for (int x = Mathf.Min(start.x + startSize.x/2, end.x + endSize.x/2) ; x <= Mathf.Max(start.x - startSize.x/2, end.x - endSize.x/2); x++)
            {
                InstantiateGrid(x, startY);
            }
        }
        // ���� ���� ����
        else if (isHorizontalOverlap)
        {
            int startX= Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2) + Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2);
            startX = startX / 2;
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
            startY = startY / 2;
            for (int x = (int)Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2); x <= (int)Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2); x++)
            {
                AddHallwayWidth(x, startY);
            }
        }
        // ���� ���� ����
        else if (isHorizontalOverlap)
        {
            int startX = Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2) + Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2);
            startX = startX / 2;
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
                        else if(map[checkY, checkX] == hallwayId || rooms[map[checkY, checkX]].activeSelf) // ���� �ƴ� ���� Ȯ��
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
    private void InstantiateGrid(int x, int y)
    {
        if (map[y - minY, x - minX] == -1) // �ش� ��ġ�� �̹� �׸��尡 ���� ��쿡�� ����
        {
            GameObject grid = Instantiate(GridPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
            grid.GetComponent<SpriteRenderer>().color = Color.black;
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

    #endregion

    #region AUTO TILING

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    
    [Header("Tiles")]
    [SerializeField] private Tile wall_Top_Left;
    [SerializeField] private Tile wall_Top_Right;
    [SerializeField] private Tile wall_Bottom_Left;
    [SerializeField] private Tile wall_Bottom_Right;
    [SerializeField] private Tile wall_Bottom;
    [SerializeField] private Tile wall_Top;
    [SerializeField] private Tile wall_Right;
    [SerializeField] private Tile wall_Left;
    [SerializeField] private Tile floor;

    const int TopMask = (1 << 1) | (1 << 3) | (1 << 5);
    const int BottomMask = (1 << 3) | (1 << 5) | (1 << 7);
    const int LeftMask = (1 << 1) | (1 << 3) | (1 << 7);
    const int RightMask = (1 << 1) | (1 << 5) | (1 << 7);
    const int TopLeftMask = (1 << 3) | (1 << 1) | (1 << 0);
    const int TopRightMask = (1 << 1) | (1 << 2) | (1 << 5);
    const int BottomLeftMask = (1 << 3) | (1 << 6) | (1 << 7);
    const int BottomRightMask = (1 << 5) | (1 << 7) | (1 << 8);

    const int TopLeftMask_1 = (1 << 3) | (1 << 1) | (1 << 0);
    const int TopRightMask_1 = (1 << 1) | (1 << 2) | (1 << 5);
    const int BottomLeftMask_1 = (1 << 3) | (1 << 6) | (1 << 7);
    const int BottomRightMask_1 = (1 << 5) | (1 << 7) | (1 << 8);

    const int TopMatch = 1 << 1;
    const int BottomMatch = 1 << 7;
    const int LeftMatch = 1 << 3;
    const int RightMatch = 1 << 5;
    const int TopLeftMatch = 1 << 0;
    const int TopRightMatch = 1 << 2;
    const int BottomLeftMatch = 1 << 0;
    const int BottomRightMatch = 1 << 8;

    const int TopLeftMatch_1 = 1 << 0;
    const int TopRightMatch_1 = 1 << 2;
    const int BottomLeftMatch_1 = 1 << 0;
    const int BottomRightMatch_1 = 1 << 8;


    private void MapArrNormalization()
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] < 0 || (map[i, j] != hallwayId && !rooms[map[i, j]].activeSelf)) map[i, j] = (int)Define.GridType.None;
                else map[i, j] = (int)Define.GridType.HallWay;
            }
        }
    }

    private void AutoTiling()
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] == 0)
                {
                    PlaceTile(j, i, 2);
                }
                else
                {
                    PlaceTile(j, i, 1);
                }
            }
        }
    }

    // tileType : 1�̸� �ٴ�, 2�� ��
    private void PlaceTile(int x, int y, int tileType)
    {
        Tile tile = null;
        // TODO : ��ġ ���� �ؾ���
        Vector3Int tilePos = new Vector3Int(x, y, 0);

        switch (tileType)
        {
            case 1: // floor
                tile = floor;
                wallTilemap.SetTile(tilePos, tile);
                break;
            case 2: // wall
                tile = DetermineWallTile(x, y);
                if (tile != null) wallTilemap.SetTile(tilePos, tile);
                break;
            default:
                break;
        }


    }
    private Tile DetermineWallTile(int x, int y)
    {
        // ���� ���
        int pattern = CalculatePattern(x, y);

        // ���Ͽ� ���� Ÿ�� ����
        if (Matches(pattern, TopMask, TopMatch)) return wall_Top;
        if (Matches(pattern, BottomMask, BottomMatch)) return wall_Bottom;
        if (Matches(pattern, LeftMask, LeftMatch)) return wall_Left;
        if (Matches(pattern, RightMask, RightMatch)) return wall_Right;
        if (Matches(pattern, TopLeftMask, TopLeftMatch)) return wall_Top_Left;
        if (Matches(pattern, TopRightMask, TopRightMatch)) return wall_Top_Right;
        if (Matches(pattern, BottomLeftMask, BottomLeftMatch)) return wall_Bottom_Left;
        if (Matches(pattern, BottomRightMask, BottomRightMatch)) return wall_Bottom_Right;

        // �⺻��
        return null;
    }

    int[] surrX = { 1, 0, -1, 1, 0, -1, 1, 0, -1 };
    int[] surrY = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
    int CalculatePattern(int x, int y)
    {
        int pattern = 0;
        int bitIndex = 0;

        // �ֺ� Ÿ���� �˻��ϴ� ���� ���� (��� ���ʺ��� �ð� ��������)

        for (int i = 0; i < surrX.Length; i++)
        {
            int checkX = x + surrX[i];
            int checkY = y + surrY[i];

            // �� ���� ������ �˻�
            if (checkX >= 0 && checkX < map.GetLength(1) && checkY >= 0 && checkY < map.GetLength(0))
            {
                if (map[checkY, checkX] == (int)Define.GridType.MainRoom || map[checkY, checkX] == (int)Define.GridType.HallWay)
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
}
