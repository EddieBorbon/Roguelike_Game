using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CellObject
{
    public float moveSpeed = 5f; 
    public float moveDelay = 0.5f;

    private Animator m_Animator;
    private List<Vector2Int> m_PathToPlayer; 

    public int m_enemyHealth = 3;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Animator not found on enemy.");
        }
        PlayerController.OnPlayerMoved += OnPlayerMoved;
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerMoved -= OnPlayerMoved;
    }

    private void OnPlayerMoved()
    {
        StartCoroutine(MoveAfterDelay());
    }

    private IEnumerator MoveAfterDelay()
    {
        yield return new WaitForSeconds(moveDelay); 

        var playerCell = GameManager.Instance.playerController.m_CellPosition;

        m_PathToPlayer = FindPath(m_Cell, playerCell);

        if (m_PathToPlayer != null && m_PathToPlayer.Count > 0)
        {
            if (m_PathToPlayer.Count == 1)
            {
               Debug.Log("The enemy is one cell away from the player. It does not move.");
            }
            else
            {
                var nextCell = m_PathToPlayer[0]; 
                yield return MoveTo(nextCell);
            }
        }
        else
        {
           Debug.Log("No valid route to the player was found.");
        }
        CheckIfPlayerIsAdjacent();
    }

    private void CheckIfPlayerIsAdjacent()
    {
        var playerCell = GameManager.Instance.playerController.m_CellPosition;
        bool isPlayerAdjacent = IsCellAdjacent(m_Cell, playerCell);

        if (isPlayerAdjacent)
        {
            Debug.Log("The player is on an adjacent cell.");
            PlayAttackAnimation(); 
        }
        else
        {
           Debug.Log("The player is NOT on an adjacent cell.");
        }
    }

    private void PlayAttackAnimation()
    {
        if (m_Animator != null)
        {
            EnemyAttack();
        }
        else
        {
            Debug.LogError("Animator not assigned to the enemy.");
        }
    }

    private bool IsCellAdjacent(Vector2Int cell, Vector2Int targetCell)
    {
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
        var openSet = new List<Vector2Int>(); 
        var closedSet = new HashSet<Vector2Int>(); 
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(); 
        var gScore = new Dictionary<Vector2Int, float>(); 
        var fScore = new Dictionary<Vector2Int, float>(); 

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);

        while (openSet.Count > 0)
        {
            var current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (fScore[openSet[i]] < fScore[current])
                {
                    current = openSet[i];
                }
            }

            if (current == target)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                var tentativeGScore = gScore[current] + 1;

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
        return null;
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
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        var neighbors = new List<Vector2Int>();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            var neighbor = cell + dir;

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

        if (targetCell == null || !targetCell.Passable || targetCell.ContainedObject != null)
        {
            yield break;
        }

        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        targetCell.ContainedObject = this;
        m_Cell = coord;

        float elapsedTime = 0;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = board.CellToWorld(coord);

        while (elapsedTime < 1f / moveSpeed)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime * moveSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; 
    }
    public void EnemyAttack()
    {
        int damageAmount = 1;
        int playerDefense = GameManager.Instance.playerController.Defense;
        m_Animator.SetTrigger("Attack");
        GameManager.Instance.playerController.TakeDamage();
        GameManager.Instance.ChangeHealth(damageAmount);
    }
    public void TakeDamage(int playerStrength)
    {
        Debug.Log("Enemy took damage.");
        m_enemyHealth -= playerStrength;
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