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

    private int[,] map; // 2차원 배열 맵

    private int minX = int.MaxValue, minY = int.MaxValue;
    private int maxX = int.MinValue, maxY = int.MinValue;



    private void Start()
    {
        StartCoroutine(MapGenerateCoroutine());
    }




    #region PROCEDURE MAP GENERATE

    /*  
     *  방 만들어지는 과정 시뮬하기위한 코루틴
     *  물리연산 되는동안 3.5~5초는 기다려야함
     */
    private IEnumerator MapGenerateCoroutine()
    {

        // 방 랜덤생성
        for (int i = 0; i < generateRoomCnt; i++)
        {
            rooms.Add(Instantiate(GridPrefab, GetRandomPointInCircle(10), new Quaternion(0, 0, 0, 0)));
            rooms[i].transform.localScale = GetRandomScale();

            yield return new WaitForSeconds(0.03f);
        }

        // 물리연산을 하기 위해 Dynamic
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
    /// 물리연산 후에 위치를 정수로 변환하기 위해 사용
    /// </summary>
    /// <param name="n">변환할 값</param>
    /// <param name="m">그리드 간격 (2이면 return값이 짝수만 나옴)</param>
    /// <returns></returns>
    private int RoundPos(float n, int m)
    {
        return Mathf.FloorToInt(((n + m - 1) / m)) * m;
    }

    private void FindMainRooms(int roomCount)
    {
        // 각 방의 크기, 비율, 인덱스를 저장할 리스트 생성
        List<(float size, int index)> roomSizes = new List<(float size, int index)>();

        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            rooms[i].transform.position = new Vector3(RoundPos(rooms[i].transform.position.x, PIXEL), RoundPos(rooms[i].transform.position.y, PIXEL), 1);

            Vector3 scale = rooms[i].transform.localScale;
            float size = scale.x * scale.y; // 방의 크기(넓이) 계산
            float ratio = scale.x / scale.y; // 방의 비율 계산
            if (ratio > 2f || ratio < 0.5f) continue; // 1:2 또는 2:1 비율을 초과하는 경우 건너뛰기
            roomSizes.Add((size, i)); // 크기, 비율, 원래 인덱스 저장
        }

        // 방의 크기에 따라 내림차순으로 정렬
        var sortedRooms = roomSizes.OrderByDescending(room => room.size).ToList();

        // 모든 방을 일단 비활성화
        foreach (var room in rooms)
        {
            room.SetActive(false);
        }

        // 비율 조건을 만족하는 방 선택 및 처리
        int count = 0;
        foreach (var roomInfo in sortedRooms)
        {
            if (count >= roomCount) break; // 선택 후 종료
            GameObject room = rooms[roomInfo.index];
            SpriteRenderer renderer = room.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.black;
            }
            room.SetActive(true);
            points.Add(new Delaunay.Vertex((int)room.transform.position.x, (int)room.transform.position.y)); // points 리스트에 추가
            count++;
        }
    }

    // 맵 정보를 2차원 배열에 저장
    private void GenerateMapArr()
    {
        // 배열 크기 결정을 위한 최소/최대 좌표 초기화

        // 최소/최대 좌표 탐색
        foreach (var room in rooms)
        {
            Vector3 pos = room.transform.position;
            Vector3 scale = room.transform.localScale;

            minX = Mathf.Min(minX, Mathf.FloorToInt(pos.x - scale.x));
            minY = Mathf.Min(minY, Mathf.FloorToInt(pos.y - scale.y));
            maxX = Mathf.Max(maxX, Mathf.CeilToInt(pos.x + scale.x));
            maxY = Mathf.Max(maxY, Mathf.CeilToInt(pos.y + scale.y));
        }

        // 배열 크기 계산 및 초기화
        int width = maxX - minX;
        int height = maxY - minY;
        map = new int[height, width];

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++) map[i, j] = -1;

        // 배열에 GameObject 저장
        for (int i = 0; i < rooms.Count; i++)
        {
            Vector3 pos = rooms[i].transform.position;
            Vector3 scale = rooms[i].transform.localScale;

            // GameObject의 크기와 위치를 고려하여 배열에 저장
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

    // 들로네 삼각분할, 최소 스패닝 트리로 방 연결
    // + 복도 생성
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
            // 시작점과 끝점 사이의 복도 생성
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

        // 수평 복도 생성
        if (isVerticalOverlap)
        {
            int startY = Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y/2) + Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y/2);
            startY = startY / 2;
            for (int x = Mathf.Min(start.x + startSize.x/2, end.x + endSize.x/2) ; x <= Mathf.Max(start.x - startSize.x/2, end.x - endSize.x/2); x++)
            {
                InstantiateGrid(x, startY);
            }
        }
        // 수직 복도 생성
        else if (isHorizontalOverlap)
        {
            int startX= Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2) + Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2);
            startX = startX / 2;
            for (int y = Mathf.Min(start.y + startSize.y/2, end.y + endSize.y/2); y <= Mathf.Max(start.y - startSize.y/2, end.y - endSize.y/2); y++)
            {
                InstantiateGrid(startX, y);
            }
        }
        else // 'ㄴ'자 또는 'ㄱ'자 복도 생성
        {
            // 전체 Delaunay.Vertex의 중간점 계산
            int mapCenterX = map.GetLength(0) / 2;
            int mapCenterY = map.GetLength(1) / 2;

            // start와 end 사이의 중간점 계산
            int midX = (start.x + end.x) / 2;
            int midY = (start.y + end.y) / 2;

            // 중간점이 전체 맵의 중심점에 비해 어느 사분면에 위치하는지 파악
            int quadrant = DetermineQuadrant(midX - mapCenterX - minX, midY - mapCenterY - minY);

            // 사분면과 start와 end의 상대적 위치에 따라 'ㄴ' 혹은 'ㄱ' 형태 결정, 항상 start의 x가 작음.
            if (quadrant == 2 || quadrant == 3)
            {
                // 사분면이 2 또는 3인 경우
                CreateStraightPath(start.x, start.y,end.x, start.y); // 가로로 먼저 이동
                CreateStraightPath(end.x, start.y, end.x, end.y); // 다음 세로로 이동
            }
            else if (quadrant == 1 || quadrant == 4)
            {
                // 사분면이 1 또는 4인 경우
                CreateStraightPath(start.x, start.y,start.x, end.y); // 세로로 먼저 이동
                CreateStraightPath(start.x, end.y, end.x, end.y); // 다음 가로로 이동
            }
        }
    }
    private void CreateCorridorWidth(Delaunay.Vertex start, Delaunay.Vertex end, Vector2Int startSize, Vector2Int endSize)
    {
        bool isHorizontalOverlap = Mathf.Abs(start.x - end.x) < ((startSize.x + endSize.x) / 2f - overlapOffset);
        bool isVerticalOverlap = Mathf.Abs(start.y - end.y) < ((startSize.y + endSize.y) / 2f - overlapOffset);

        // 수평 복도 생성
        if (isVerticalOverlap)
        {

            int startY = Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2) + Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2);
            startY = startY / 2;
            for (int x = (int)Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2); x <= (int)Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2); x++)
            {
                AddHallwayWidth(x, startY);
            }
        }
        // 수직 복도 생성
        else if (isHorizontalOverlap)
        {
            int startX = Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2) + Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2);
            startX = startX / 2;
            for (int y = (int)Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2); y <= (int)Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2); y++)
            {
                AddHallwayWidth(startX, y);
            }
        }
        else // 'ㄴ'자 또는 'ㄱ'자 복도 생성
        {
            // 전체 Delaunay.Vertex의 중간점 계산 (여기서는 단순화를 위해 맵의 중심을 사용)
            int mapCenterX = map.GetLength(0) / 2;
            int mapCenterY = map.GetLength(1) / 2;

            // start와 end 사이의 중간점 계산
            int midX = (start.x + end.x) / 2;
            int midY = (start.y + end.y) / 2;

            // 중간점이 전체 맵의 중심점에 비해 어느 사분면에 위치하는지 파악
            int quadrant = DetermineQuadrant(midX - mapCenterX - minX, midY - mapCenterY - minY);

            // 사분면과 start와 end의 상대적 위치에 따라 'ㄴ' 혹은 'ㄱ' 형태 결정
            if (quadrant == 2 || quadrant == 3)
            {
                // 사분면이 1 또는 3인 경우
                CreateStraightHall(start.x, start.y, end.x, start.y); // 가로로 먼저 이동
                CreateStraightHall(end.x, start.y, end.x, end.y); // 다음 세로로 이동
            }
            else if (quadrant == 1 || quadrant == 4)
            {
                // 사분면이 2 또는 4인 경우
                CreateStraightHall(start.x, start.y, start.x, end.y); // 세로로 먼저 이동
                CreateStraightHall(start.x, end.y, end.x, end.y); // 다음 가로로 이동
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
        return 0; // 이 경우는 발생하지 않음
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


                int nonWallCount = 0; // 벽이 아닌 공간의 수를 카운트

                // 나를 포함한 주위 9칸 검사
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        int checkX = x + offsetX;
                        int checkY = y + offsetY;

                        // 배열 범위를 벗어나면 벽으로 간주
                        if (checkX < 0 || checkX >= maxX - minX || checkY < 0 || checkY >= maxY - minY)
                        {
                            continue;
                        }
                        else if (map[checkY, checkX] == -1) continue;
                        else if(map[checkY, checkX] == hallwayId || rooms[map[checkY, checkX]].activeSelf) // 벽이 아닌 공간 확인
                        {
                            nonWallCount++;
                        }
                    }
                }

                // 주변에 벽이 아닌 공간이 5칸 이상이면 일반 공간으로 변경
                if (nonWallCount >= 5)
                {
                    map[y, x] = hallwayId; // 배열에 hallwayId 저장

                    GameObject grid = Instantiate(GridPrefab, new Vector3(x + minX + 0.5f, y +minY + 0.5f, 0), Quaternion.identity);
                    grid.GetComponent<SpriteRenderer>().color = Color.black;
                    map[y, x] = hallwayId;
                }
            }
        }
    }
    private void InstantiateGrid(int x, int y)
    {
        if (map[y - minY, x - minX] == -1) // 해당 위치에 이미 그리드가 없는 경우에만 생성
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

    const int TopLeftMask = 1 << 4;
    const int TopRightMask = 1 << 5;
    const int BottomLeftMask = 1 << 6;
    const int BottomRightMask = 1 << 0;

    const int TopMatch = 1 << 1;
    const int BottomMatch = 1 << 7;
    const int LeftMatch = 1 << 3;
    const int RightMatch = 1 << 5;

    const int TopLeftMatch = 1 << 5;
    const int TopRightMatch = 1 << 0;
    const int BottomLeftMatch = 1 << 0;
    const int BottomRightMatch = 1 << 0;


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

    // tileType : 1이면 바닥, 2면 벽
    private void PlaceTile(int x, int y, int tileType)
    {
        Tile tile = null;
        // TODO : 위치 조정 해야함
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
        // 패턴 계산
        int pattern = CalculatePattern(x, y);

        // 패턴에 따른 타일 결정
        if (Matches(pattern, TopMask, TopMatch)) return wall_Top;
        if (Matches(pattern, BottomMask, BottomMatch)) return wall_Bottom;
        if (Matches(pattern, LeftMask, LeftMatch)) return wall_Left;
        if (Matches(pattern, RightMask, RightMatch)) return wall_Right;
        //if (Matches(pattern, TopLeftMask, TopLeftMatch)) return wall_Top_Left;
        //if (Matches(pattern, TopRightMask, TopRightMatch)) return wall_Top_Right;
        //if (Matches(pattern, BottomLeftMask, BottomLeftMatch)) return wall_Bottom_Left;
        //if (Matches(pattern, BottomRightMask, BottomRightMatch)) return wall_Bottom_Right;

        // 기본값
        return null;
    }

    int[] surrX = { 1, 0, -1, 1, 0, -1, 1, 0, -1 };
    int[] surrY = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
    int CalculatePattern(int x, int y)
    {
        int pattern = 0;
        int bitIndex = 0;

        // 주변 타일을 검사하는 순서 정의 (상단 왼쪽부터 시계 방향으로)

        for (int i = 0; i < surrX.Length; i++)
        {
            int checkX = x + surrX[i];
            int checkY = y + surrY[i];

            // 맵 범위 내에서 검사
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
