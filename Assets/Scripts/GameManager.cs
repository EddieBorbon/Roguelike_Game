using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoardManager boardManager;
    public PlayerController playerController;

    public TurnManager turnManager { get; private set; }
    private int m_FoodAmount = 100;
    private int m_StrengthAmount = 0;
    private int m_SpeedAmount = 0;
    private int m_DefenseAmount = 0;
    private int m_HealthAmount = 10;

    public UIDocument UIDoc;
    private Label m_FoodLabel;
    private Label m_StrengthLabel;
    private Label m_DefenseLabel;
    private Label m_SpeedLabel;
    private Label m_HealthLabel;

    private int m_CurrentLevel = 1;

    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;

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

        StartNewGame(); // Aquí se llama a StartNewGame
    }

    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_CurrentLevel = 1;
        m_FoodAmount = 20;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        boardManager.Clean();
        boardManager.Init(m_CurrentLevel); // Pasar el nivel actual al inicializar el tablero

        playerController.Init();
        playerController.Spawn(boardManager, new Vector2Int(1, 1));
    }

    public void NewLevel()
    {
        m_CurrentLevel++;
        boardManager.Clean();
        boardManager.Init(m_CurrentLevel); // Pasar el nuevo nivel al inicializar el tablero
        playerController.Spawn(boardManager, new Vector2Int(1, 1));
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
    public void ChangeDefense(int amount)
    {
        m_DefenseAmount += amount;
        m_DefenseLabel.text = "Defense: " + m_DefenseAmount;
    }
    public void ChangeHealth(int damageAmount, int defense)
    {
        // Calcular el daño neto teniendo en cuenta la defensa
        int netDamage = Mathf.Max(1, damageAmount - defense);

        // Reducir la salud solo si el daño neto es suficiente
        if (netDamage > 0)
        {
            m_HealthAmount -= netDamage;
            m_HealthLabel.text = "Health: " + m_HealthAmount;

            if (m_HealthAmount <= 0)
            {
                GameOverManager();
            }
        }
        else
        {
            Debug.Log("El jugador ha bloqueado el daño gracias a su defensa.");
        }
    }
    public void GameOverManager()
    {
        playerController.GameOver();
        m_GameOverPanel.style.visibility = Visibility.Visible;
        m_GameOverMessage.text = "Game Over!\n\nYou traveled through " + m_CurrentLevel + " levels";
    }
}