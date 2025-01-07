using System;

public class TurnManager
{
    public event Action OnTick; 
    private int m_CurrentTurn = 0; 
    private int m_TurnsPerRound = 10; 
    public bool IsNewRound { get; private set; } 

    public void Tick()
    {
        m_CurrentTurn++;

        if (m_CurrentTurn % m_TurnsPerRound == 0)
        {
            IsNewRound = true;
        }
        else
        {
            IsNewRound = false;
        }

        OnTick?.Invoke();
    }

    public void Reset()
    {
        m_CurrentTurn = 0;
        IsNewRound = false;
    }
}