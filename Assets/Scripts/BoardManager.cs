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

    public int BaseWidth = 5; // Tamaño base del tablero
    public int BaseHeight = 5; // Tamaño base del tablero
    public int LevelScaleFactor = 2; // Factor de escala por nivel
    public Tile[] GroundTiles;
    public Tile[] WallTiles;
    public List<Vector2Int> m_EmptyCellsList;
    public Grid m_Grid;
    public FoodObject[] FoodPrefabs;
    public WallObject[] wallPrefabs;
    public StrengthItem strengthItemPrefab; // Prefab del ítem de fuerza
    public DefenseItem defenseItemPrefab; // Prefab del ítem de defensa
    public SpeedItem speedItemPrefab; // Prefab del ítem de velocidad


    public int BaseMinFoodCount = 3; // Cantidad mínima base de comida
    public int BaseMaxFoodCount = 7; // Cantidad máxima base de comida
    public PlayerController Player;

    public ExitCellObject exitCellPrefab;

    public int BaseMinEnemyCount = 1; // Cantidad mínima base de enemigos
    public int BaseMaxEnemyCount = 5; // Cantidad máxima base de enemigos

    public Enemy[] enemiesPrefabs;

    private Enemy[] Enemies;

    public int Width { get; private set; } // Ancho dinámico
    public int Height { get; private set; } // Alto dinámico

    public PolygonCollider2D boardConfiner; // Referencia al Polygon Collider 2D

    public CameraController cameraController;



    // Inicializar el tablero según el nivel
    public void Init(int level)
    {
        // Calcular el tamaño del tablero basado en el nivel
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
        m_EmptyCellsList.Remove(new Vector2Int(1, 1)); // Eliminar la posición inicial del jugador

        // Generar la salida en una ubicación aleatoria
        GenerateExit();

        // Generar paredes, comida y enemigos basados en el nivel
        GenerateWall(level);
        GenerateFood(level);
        GenerateEnemies(level);

        // Generar ítems de estadísticas
        GenerateStatsItems();

        // Ajustar el Polygon Collider 2D para que coincida con el tablero
        AdjustBoardConfiner(Width, Height);
    }

    public void AdjustBoardConfiner(int width, int height)
    {
        if (boardConfiner == null)
        {
            Debug.LogError("No se ha asignado un Polygon Collider 2D como confiner.");
            return;
        }

        // Calcular las coordenadas del collider
        Vector2[] colliderPoints = new Vector2[4];
        colliderPoints[0] = new Vector2(0, 0); // Esquina inferior izquierda
        colliderPoints[1] = new Vector2(width, 0); // Esquina inferior derecha
        colliderPoints[2] = new Vector2(width, height); // Esquina superior derecha
        colliderPoints[3] = new Vector2(0, height); // Esquina superior izquierda

        // Asignar los puntos al collider
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
        int foodCount = Random.Range(BaseMinFoodCount + level, BaseMaxFoodCount + level); // Aumentar la cantidad de comida según el nivel
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
        int wallCount = Random.Range(6 + level, 10 + level); // Aumentar la cantidad de paredes según el nivel
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
        //no board data, so exit early, nothing to clean
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

        // Limpiar el arreglo de enemigos
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

    // Generar enemigos basados en el nivel
    private void GenerateEnemies(int level)
    {
        int enemyCount = Random.Range(BaseMinEnemyCount + level, BaseMaxEnemyCount + level); // Aumentar la cantidad de enemigos según el nivel
        Enemies = new Enemy[enemyCount]; // Inicializar el arreglo de enemigos instanciados

        for (int i = 0; i < enemyCount; i++)
        {
            if (m_EmptyCellsList.Count > 0)
            {
                int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
                Vector2Int coord = m_EmptyCellsList[randomIndex];

                m_EmptyCellsList.RemoveAt(randomIndex);

                // Seleccionar un prefab de enemigo aleatorio
                Enemy randomEnemyPrefab = enemiesPrefabs[Random.Range(0, enemiesPrefabs.Length)];
                Enemy newEnemy = Instantiate(randomEnemyPrefab);

                AddObject(newEnemy, coord);
                Enemies[i] = newEnemy; // Agregar el enemigo al arreglo
            }
        }
    }
    private void GenerateExit()
    {
        if (m_EmptyCellsList.Count > 0)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int exitCoord = m_EmptyCellsList[randomIndex];

            // Añadir la salida en la coordenada aleatoria
            AddObject(Instantiate(exitCellPrefab), exitCoord);
            m_EmptyCellsList.Remove(exitCoord); // Eliminar la celda de la lista de celdas vacías
        }
        else
        {
            Debug.LogError("No hay celdas disponibles para generar la salida.");
        }
    }
    // Generar ítems de estadísticas
    private void GenerateStatsItems()
    {
        // Generar un ítem de fuerza
        GenerateRandomItem(strengthItemPrefab);

        // Generar un ítem de defensa
        GenerateRandomItem(defenseItemPrefab);

        // Generar un ítem de velocidad
        GenerateRandomItem(speedItemPrefab);
    }
     // Generar un ítem en una celda aleatoria
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
            Debug.LogWarning("No hay celdas disponibles para generar un ítem.");
        }
    }
}