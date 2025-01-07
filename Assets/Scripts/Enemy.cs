using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CellObject
{
    public float moveSpeed = 5f; // Velocidad de movimiento
    public float moveDelay = 0.5f; // Retraso antes de moverse

    private Animator m_Animator; // Referencia al Animator
    private List<Vector2Int> m_PathToPlayer; // Ruta hacia el jugador

    public int m_enemyHealth = 3;

    private void Start()
    {
        // Obtener la referencia al Animator
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Animator no encontrado en el enemigo.");
        }

        // Suscribirse al evento de movimiento del jugador
        PlayerController.OnPlayerMoved += OnPlayerMoved;
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento de movimiento del jugador
        PlayerController.OnPlayerMoved -= OnPlayerMoved;
    }

    private void OnPlayerMoved()
    {
        // Iniciar una corrutina para mover al enemigo después de un retraso
        StartCoroutine(MoveAfterDelay());
    }

    private IEnumerator MoveAfterDelay()
    {
        yield return new WaitForSeconds(moveDelay); // Esperar el retraso

        // Obtener la posición del jugador
        var playerCell = GameManager.Instance.playerController.m_CellPosition;

        // Calcular la ruta hacia el jugador usando A*
        m_PathToPlayer = FindPath(m_Cell, playerCell);

        // Si hay una ruta válida, mover al enemigo una celda
        if (m_PathToPlayer != null && m_PathToPlayer.Count > 0)
        {
            // Verificar si el enemigo está a una celda de distancia del jugador
            if (m_PathToPlayer.Count == 1)
            {
              //  Debug.Log("El enemigo está a una celda del jugador. No se mueve.");
            }
            else
            {
                // Mover al enemigo a la siguiente celda en la ruta
                var nextCell = m_PathToPlayer[0]; // La primera celda en la ruta es la siguiente
                yield return MoveTo(nextCell); // Mover y esperar a que termine el movimiento
            }
        }
        else
        {
           // Debug.Log("No se encontró una ruta válida hacia el jugador.");
        }

        // Después de moverse, verificar si el jugador está en una celda adyacente
        CheckIfPlayerIsAdjacent();
    }

    private void CheckIfPlayerIsAdjacent()
    {
        // Obtener la posición del jugador
        var playerCell = GameManager.Instance.playerController.m_CellPosition;

        // Verificar si el jugador está en una celda adyacente
        bool isPlayerAdjacent = IsCellAdjacent(m_Cell, playerCell);

        // Mostrar un mensaje en la consola
        if (isPlayerAdjacent)
        {
       //     Debug.Log("El jugador está en una celda adyacente.");
            PlayAttackAnimation(); // Ejecutar la animación de ataque
        }
        else
        {
          //  Debug.Log("El jugador NO está en una celda adyacente.");
        }
    }

    private void PlayAttackAnimation()
    {
        // Activar el trigger "Attack" en el Animator
        if (m_Animator != null)
        {
            EnemyAttack();
        }
        else
        {
            Debug.LogError("Animator no asignado en el enemigo.");
        }
    }

    private bool IsCellAdjacent(Vector2Int cell, Vector2Int targetCell)
    {
        // Verificar si la celda objetivo está en una de las 4 direcciones adyacentes
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            if (cell + dir == targetCell)
            {
                return true;
            }
        }

        return false;
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        var openSet = new List<Vector2Int>(); // Nodos por explorar
        var closedSet = new HashSet<Vector2Int>(); // Nodos ya explorados
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // De dónde vino cada nodo
        var gScore = new Dictionary<Vector2Int, float>(); // Costo desde el inicio
        var fScore = new Dictionary<Vector2Int, float>(); // Costo total estimado

        // Inicializar el inicio
        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);

        while (openSet.Count > 0)
        {
            // Obtener el nodo con el menor fScore
            var current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (fScore[openSet[i]] < fScore[current])
                {
                    current = openSet[i];
                }
            }

            // Si llegamos al objetivo, reconstruir la ruta
            if (current == target)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // Explorar vecinos
            foreach (var neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                var tentativeGScore = gScore[current] + 1; // Costo de moverse a un vecino

                if (!openSet.Contains(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, target);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null; // No se encontró una ruta
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int>();

        while (cameFrom.ContainsKey(current))
        {
            path.Insert(0, current);
            current = cameFrom[current];
        }

        return path;
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Distancia Manhattan (ideal para grids)
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        var neighbors = new List<Vector2Int>();

        // Vecinos en las 4 direcciones (arriba, abajo, izquierda, derecha)
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            var neighbor = cell + dir;

            // Verificar si el vecino es pasable y está dentro del tablero
            if (IsCellPassable(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private bool IsCellPassable(Vector2Int cell)
    {
        var board = GameManager.Instance.boardManager;
        var targetCell = board.GetCellData(cell);

        // Verificar si la celda es válida, pasable y no está ocupada por el jugador
        return targetCell != null && targetCell.Passable && targetCell.ContainedObject == null;
    }

    private IEnumerator MoveTo(Vector2Int coord)
    {
        if (GameManager.Instance == null || GameManager.Instance.boardManager == null)
        {
            yield break;
        }

        var board = GameManager.Instance.boardManager;
        var targetCell = board.GetCellData(coord);

        // Verificar si la celda objetivo es válida
        if (targetCell == null || !targetCell.Passable || targetCell.ContainedObject != null)
        {
            yield break;
        }

        // Liberar la celda actual
        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        // Ocupar la celda objetivo
        targetCell.ContainedObject = this;
        m_Cell = coord;

        // Mover suavemente al enemigo a la nueva posición
        float elapsedTime = 0;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = board.CellToWorld(coord);

        while (elapsedTime < 1f / moveSpeed)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime * moveSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Asegurarse de que llegue exactamente a la posición objetivo
    }
    public void EnemyAttack()
    {
        int damageAmount = 1;
        int playerDefense = GameManager.Instance.playerController.Defense;
        m_Animator.SetTrigger("Attack");
        GameManager.Instance.playerController.TakeDamage();
        GameManager.Instance.ChangeHealth(damageAmount, playerDefense);
    }
    public void TakeDamage(int playerStrength)
    {
       // Debug.Log("Enemigo recibió daño.");
        m_enemyHealth -= playerStrength;
        // m_Health -= Strength; // Usar la fuerza del jugador para calcular el daño
        if (m_enemyHealth <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            EnemyAttack();
        }
    }
}