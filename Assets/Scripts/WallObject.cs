using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    public Tile obstacleTile; // Tile for the wall
    public Tile damagedTile;  // Tile for the damaged wall
    public int MaxHealth = 3; // Maximum health of the wall

    private int m_HealthPoint; // Current health of the wall
    private Tile m_OriginalTile; // Original tile of the cell (e.g., ground tile)

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        m_HealthPoint = MaxHealth; // Initialize health
        m_OriginalTile = GameManager.Instance.boardManager.GetCellTile(cell); // Save the original tile
        GameManager.Instance.boardManager.SetCellTile(cell, obstacleTile); // Set the wall tile
    }

    public override bool PlayerWantsToEnter()
    {
        GameManager.Instance.playerController.Attack();
        m_HealthPoint -= 1; // Reduce health when the player interacts with the wall

        if (m_HealthPoint > 0)
        {
            // If the wall is still alive, update its appearance if damaged
            if (m_HealthPoint == 1 && damagedTile != null)
            {
                GameManager.Instance.boardManager.SetCellTile(m_Cell, damagedTile); // Change to damaged tile
                
            }
            return false; // Player cannot pass through the wall
        }
        else
        {
            // If the wall is destroyed, restore the original tile and destroy the wall object
            GameManager.Instance.boardManager.SetCellTile(m_Cell, m_OriginalTile);
            Destroy(gameObject);
            return false; // Player cannot pass through the wall (though it's destroyed)
        }
    }
    public override bool IsPassable()
    {
        return false;
    }
}