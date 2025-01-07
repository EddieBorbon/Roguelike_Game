using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }
    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;

    public int BaseWidth = 5; 
    public int BaseHeight = 5; 
    public int LevelScaleFactor = 2; 
    public Tile[] GroundTiles;
    public Tile[] WallTiles;
    public List<Vector2Int> m_EmptyCellsList;
    public Grid m_Grid;
    public FoodObject[] FoodPrefabs;
    public WallObject[] wallPrefabs;
    public StrengthItem strengthItemPrefab; 
    public DefenseItem defenseItemPrefab; 
    public SpeedItem speedItemPrefab; 


    public int BaseMinFoodCount = 3; 
    public int BaseMaxFoodCount = 7; 
    public PlayerController Player;

    public ExitCellObject exitCellPrefab;

    public int BaseMinEnemyCount = 1; 
    public int BaseMaxEnemyCount = 5; 

    public Enemy[] enemiesPrefabs;

    private Enemy[] Enemies;

    public int Width { get; private set; } 
    public int Height { get; private set; } 

    public PolygonCollider2D boardConfiner;

    public CameraController cameraController;

    public void Init(int level)
    {
        Width = BaseWidth + (level * LevelScaleFactor);
        Height = BaseHeight + (level * LevelScaleFactor);
        cameraController.AdjustCamera(Width, Height);
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        m_BoardData = new CellData[Width, Height];
        m_EmptyCellsList = new List<Vector2Int>();

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();
                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }
                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        m_EmptyCellsList.Remove(new Vector2Int(1, 1)); 

        GenerateExit();

        GenerateWall(level);
        GenerateFood(level);
        GenerateEnemies(level);

        GenerateStatsItems();

        AdjustBoardConfiner(Width, Height);
    }

    public void AdjustBoardConfiner(int width, int height)
    {
        if (boardConfiner == null)
        {
            Debug.LogError("No se ha asignado un Polygon Collider 2D como confiner.");
            return;
        }

        Vector2[] colliderPoints = new Vector2[4];
        colliderPoints[0] = new Vector2(0, 0); 
        colliderPoints[1] = new Vector2(width, 0); 
        colliderPoints[2] = new Vector2(width, height); 
        colliderPoints[3] = new Vector2(0, height); 

        boardConfiner.SetPath(0, colliderPoints);
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }
        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    private void GenerateFood(int level)
    {
        int foodCount = Random.Range(BaseMinFoodCount + level, BaseMaxFoodCount + level); 
        for (int i = 0; i < foodCount; i++)
        {
            if (m_EmptyCellsList.Count > 0)
            {
                int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
                Vector2Int coord = m_EmptyCellsList[randomIndex];

                m_EmptyCellsList.RemoveAt(randomIndex);
                FoodObject newFood = Instantiate(FoodPrefabs[Random.Range(0, FoodPrefabs.Length)]);
                AddObject(newFood, coord);
            }
        }
    }

    private void GenerateWall(int level)
    {
        int wallCount = Random.Range(6 + level, 10 + level); 
        for (int i = 0; i < wallCount; ++i)
        {
            if (m_EmptyCellsList.Count > 0)
            {
                int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
                Vector2Int coord = m_EmptyCellsList[randomIndex];

                m_EmptyCellsList.RemoveAt(randomIndex);
                WallObject newWall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]);
                AddObject(newWall, coord);
            }
        }
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void Clean()
    {
        if (m_BoardData == null)
            return;

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
        if (Enemies != null)
        {
            foreach (var enemy in Enemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            Enemies = null;
        }
    }

    private void GenerateEnemies(int level)
    {
        int enemyCount = Random.Range(BaseMinEnemyCount + level, BaseMaxEnemyCount + level); 
        Enemies = new Enemy[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
            if (m_EmptyCellsList.Count > 0)
            {
                int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
                Vector2Int coord = m_EmptyCellsList[randomIndex];

                m_EmptyCellsList.RemoveAt(randomIndex);

                Enemy randomEnemyPrefab = enemiesPrefabs[Random.Range(0, enemiesPrefabs.Length)];
                Enemy newEnemy = Instantiate(randomEnemyPrefab);

                AddObject(newEnemy, coord);
                Enemies[i] = newEnemy; 
            }
        }
    }
    private void GenerateExit()
    {
        if (m_EmptyCellsList.Count > 0)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int exitCoord = m_EmptyCellsList[randomIndex];

            AddObject(Instantiate(exitCellPrefab), exitCoord);
            m_EmptyCellsList.Remove(exitCoord); 
        }
        else
        {
            Debug.LogError("There are no cells available to generate the output.");
        }
    }
    private void GenerateStatsItems()
    {
        GenerateRandomItem(strengthItemPrefab);
        GenerateRandomItem(defenseItemPrefab);
        GenerateRandomItem(speedItemPrefab);
    }

    private void GenerateRandomItem(CellObject itemPrefab)
    {
        if (m_EmptyCellsList.Count > 0)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            CellObject newItem = Instantiate(itemPrefab);
            AddObject(newItem, coord);
        }
        else
        {
            Debug.LogWarning("There are no cells available to generate an item.");
        }
    }
}