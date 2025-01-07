using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private BoardManager m_Board;
    public Vector2Int m_CellPosition;
    private bool m_IsGameOver;

    public float moveSpeed = 5.0f;

    private bool m_IsMoving;
    private Vector3 m_MoveTarget;

    private Animator m_animator;
    private SpriteRenderer m_spriteRenderer; // Referencia al SpriteRenderer

    // Evento para notificar el movimiento del jugador
    public static event Action OnPlayerMoved;

    public int Strength { get; private set; } = 1; // Fuerza del jugador
    public int Defense { get; private set; } = 1; // Defensa del jugador
    public int Speed { get; private set; } = 1; // Velocidad del jugador
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_spriteRenderer = GetComponent<SpriteRenderer>(); // Obtener el SpriteRenderer

        if (m_animator == null)
        {
            Debug.LogError("Animator no encontrado en el jugador.");
        }
        if (m_spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer no encontrado en el jugador.");
        }
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        if (boardManager == null)
        {
            Debug.LogError("BoardManager no proporcionado.");
            return;
        }

        m_Board = boardManager;
        MoveTo(cell, true);
    }

    public void Init()
    {
        m_IsGameOver = false;
        m_IsMoving = false;
    }

    public void MoveTo(Vector2Int cell, bool immediate)
    {
        if (m_Board == null)
        {
            Debug.LogError("BoardManager no inicializado.");
            return;
        }

        m_CellPosition = cell;

        if (immediate)
        {
            m_IsMoving = false;
            transform.position = m_Board.CellToWorld(m_CellPosition);
        }
        else
        {
            m_IsMoving = true;
            m_MoveTarget = m_Board.CellToWorld(m_CellPosition);

            if (ObjectMove.Instance == null)
            {
                Debug.LogError("ObjectMove no inicializado.");
                return;
            }

            StartCoroutine(MoveCoroutine());
        }

        if (m_animator != null)
        {
            m_animator.SetBool("Moving", m_IsMoving);
        }
    }

    private IEnumerator MoveCoroutine()
    {
        yield return ObjectMove.Instance.SmoothMove(transform, m_MoveTarget, moveSpeed);

        m_IsMoving = false;

        if (m_animator != null)
        {
            m_animator.SetBool("Moving", false);
        }

        // Verificar si hay un objeto en la celda
        var cellData = m_Board.GetCellData(m_CellPosition);
        if (cellData != null && cellData.ContainedObject != null)
        {
            if (cellData.ContainedObject.PlayerWantsToEnter())
            {
                cellData.ContainedObject.PlayerEntered();
            }
        }

        // Disparar el evento después de mover al jugador
        OnPlayerMoved?.Invoke();
    }

    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return;
        }

        if (m_IsMoving)
        {
            return;
        }

        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

        if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
        {
            newCellTarget.y += Speed; // Mover `Speed` celdas hacia arriba
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
        {
            newCellTarget.y -= Speed; // Mover `Speed` celdas hacia abajo
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasReleasedThisFrame)
        {
            newCellTarget.x += Speed; // Mover `Speed` celdas hacia la derecha
            hasMoved = true;
            FlipSprite(false); // No voltear el sprite (derecha)
        }
        else if (Keyboard.current.leftArrowKey.wasReleasedThisFrame)
        {
            newCellTarget.x -= Speed; // Mover `Speed` celdas hacia la izquierda
            hasMoved = true;
            FlipSprite(true); // Voltear el sprite (izquierda)
        }

        if (hasMoved)
        {
            if (m_Board == null)
            {
                Debug.LogError("BoardManager no inicializado.");
                return;
            }

            // Verificar si todas las celdas en el camino son pasables
            if (IsPathClear(m_CellPosition, newCellTarget))
            {
                MoveTo(newCellTarget, false);
                GameManager.Instance.turnManager.Tick();
            }
            else
            {
                Debug.Log("El camino está bloqueado.");
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            AttackEnemy();
        }
    }

    private bool IsPathClear(Vector2Int start, Vector2Int end)
    {
        Vector2Int direction = new Vector2Int(
            Mathf.Clamp(end.x - start.x, -1, 1),
            Mathf.Clamp(end.y - start.y, -1, 1)
        );

        Vector2Int currentCell = start;

        while (currentCell != end)
        {
            currentCell += direction;

            var cellData = m_Board.GetCellData(currentCell);

            // Verificar si la celda es pasable o contiene un objeto pasable (como comida)
            if (cellData == null || !cellData.Passable || (cellData.ContainedObject != null && !cellData.ContainedObject.IsPassable()))
            {
                return false; // El camino está bloqueado
            }
        }
        return true; // El camino está despejado
    }

    public void GameOver()
    {
        m_IsGameOver = true;
    }

    public void Attack()
    {
        if (m_animator != null)
        {
            m_animator.SetTrigger("Attack");
        }
    }

    public void TakeDamage()
    {
        if (m_animator != null)
        {
            m_animator.SetTrigger("Damage");
        }
    }

    private void FlipSprite(bool flipX)
    {
        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.flipX = flipX;
        }
    }

    public void IncreaseStrength(int amount)
    {
        Strength += amount;
        Debug.Log($"Fuerza aumentada a {Strength}");
    }

    public void IncreaseDefense(int amount)
    {
        Defense += amount;
        Debug.Log($"Defensa aumentada a {Defense}");
    }

    public void IncreaseSpeed(int amount)
    {
        Speed += amount;
        Debug.Log($"Velocidad aumentada a {Speed}");
    }

    public void AttackEnemy()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Vector2Int adjacentCell = m_CellPosition + dir;
            var cellData = GameManager.Instance.boardManager.GetCellData(adjacentCell);

            if (cellData != null && cellData.ContainedObject is Enemy enemy)
            {
                // Si hay un enemigo en la celda adyacente, atacarlo
                Attack(); // Ejecutar la animación de ataque
                enemy.TakeDamage(Strength);
                Debug.Log("Jugador atacó al enemigo.");
                return; // Atacar solo a un enemigo a la vez
            }
        }
    }
}