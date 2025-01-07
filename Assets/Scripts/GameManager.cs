using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoardManager boardManager;
    public PlayerController playerController;
    public CameraController cameraController;

    public TurnManager turnManager { get; private set; }
    private int m_FoodAmount = 100;
    private int m_StrengthAmount = 0;
    private int m_SpeedAmount = 0;
    private int m_HealthAmount = 10;

    public bool m_HasTemporaryDefense = false;

    public UIDocument UIDoc;
    private Label m_FoodLabel;
    private Label m_StrengthLabel;
    private Label m_DefenseLabel;
    private Label m_SpeedLabel;
    private Label m_HealthLabel;

    private int m_CurrentLevel = 1;

    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;

    public event Action OnNewLevel;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        turnManager = new TurnManager();
        turnManager.OnTick += OnTurnHappen;

        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_StrengthLabel = UIDoc.rootVisualElement.Q<Label>("strengthLabel");
        m_DefenseLabel = UIDoc.rootVisualElement.Q<Label>("defenseLabel");
        m_SpeedLabel = UIDoc.rootVisualElement.Q<Label>("speedLabel");
        m_HealthLabel = UIDoc.rootVisualElement.Q<Label>("healthLabel");

        m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");

        m_HealthLabel.text = "Health: " + 5;
        cameraController.AdjustCamera(5, 5);

        StartNewGame(); 
    }

    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_CurrentLevel = 1;
        m_FoodAmount = 20;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        boardManager.Clean();
        boardManager.Init(m_CurrentLevel); 

        playerController.Init();
        playerController.Spawn(boardManager, new Vector2Int(1, 1));

        m_HasTemporaryDefense = false;
        ChangeDefense(false);

    }

    public void NewLevel()
    {
        m_CurrentLevel++;
        boardManager.Clean();
        boardManager.Init(m_CurrentLevel); 
        playerController.Spawn(boardManager, new Vector2Int(1, 1));
        ChangeDefense(false);
        OnNewLevel?.Invoke();
    }

    void OnTurnHappen()
    {
        ChangeFood(-1);
    }

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            GameOverManager();
        }
    }
    public void ChangeStrength(int amount)
    {
        m_StrengthAmount += amount;
        m_StrengthLabel.text = "Strength: " + m_StrengthAmount;
    }
    public void ChangeSpeed(int amount)
    {
        m_SpeedAmount += amount;
        m_SpeedLabel.text = "Speed: " + m_SpeedAmount;
    }
    public void ChangeDefense(bool activated)
    {
        if (activated)
        {
            m_DefenseLabel.text = "Defense ✓";
        }
        else
        {
            m_DefenseLabel.text = "Defense X";
        }
    }
    public void ChangeHealth(int damageAmount)
    {
        if (m_HasTemporaryDefense)
        {
            Debug.Log("Sin daño.");
            return;
        }

        m_HealthAmount -= damageAmount;
        m_HealthLabel.text = "Health: " + m_HealthAmount;

        if (m_HealthAmount <= 0)
        {
            GameOverManager();
        }
    }

    public void ActivateTemporaryDefense(int amount)
{
    m_HasTemporaryDefense = true;
}

public void GameOverManager()
{
    playerController.GameOver();
    m_GameOverPanel.style.visibility = Visibility.Visible;
    m_GameOverMessage.text = "Game Over!\n\nYou traveled through " + m_CurrentLevel + " levels";
}
}