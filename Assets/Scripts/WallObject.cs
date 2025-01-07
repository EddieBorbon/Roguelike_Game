using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    public Tile obstacleTile;
    public Tile damagedTile; 
    public int MaxHealth = 3; 

    private int m_HealthPoint; 
    private Tile m_OriginalTile; 

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        m_HealthPoint = MaxHealth; 
        m_OriginalTile = GameManager.Instance.boardManager.GetCellTile(cell);
        GameManager.Instance.boardManager.SetCellTile(cell, obstacleTile);
    }

    public override bool PlayerWantsToEnter()
    {
        TakeDamage(); 
        return false; 
    }

    public void TakeDamage()
    {
        m_HealthPoint -= 1; 

        if (m_HealthPoint > 0)
        {
            if (m_HealthPoint == 1 && damagedTile != null)
            {
                GameManager.Instance.boardManager.SetCellTile(m_Cell, damagedTile); 
            }
            Debug.Log($"Muro dañado. Salud restante: {m_HealthPoint}");
        }
        else
        {
            GameManager.Instance.boardManager.SetCellTile(m_Cell, m_OriginalTile);
            Destroy(gameObject);
            Debug.Log("Muro destruido.");
        }
    }

    public override bool IsPassable()
    {
        return false; 
    }
}