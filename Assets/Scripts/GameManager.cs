
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Panels")]
    public GameObject homePanel;
    public GameObject gameplayHUD;
    public GameObject gameOverPanel;

    [Header("UI")]
    public TMP_Text scoreText;

    [Header("Game State")]
    public bool gameStarted = false;
    public bool gameOver = false;

    [Header("Score")]
    public float score = 0f;
    public int highScore = 0;

    [Header("Audio")]
    public AudioSource homePanelAmbience;
    public AudioSource gameplayBirdAmbience;

    private const string HighScoreKey = "ZombieRunnerHighScore";

    private static bool restartIntoGame = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;

        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        score = 0f;
        gameStarted = false;
        gameOver = false;
    }

    private void Start()
    {
        if (restartIntoGame)
        {
            restartIntoGame = false;
            StartGame();
        }
        else
        {
            ShowHomeMenu();
            UpdateScoreText();
        }
    }

    private void Update()
    {
        if (!gameStarted || gameOver)
            return;

        score += Time.deltaTime;

        int currentScore = Mathf.FloorToInt(score);

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
        }

        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "Score: " + Mathf.FloorToInt(score) +
                "\nBest: " + highScore;
        }
    }

    private void SetPanel(GameObject panel, bool state)
    {
        if (panel != null)
            panel.SetActive(state);
    }

    private void StopAllAmbience()
    {
        if (homePanelAmbience != null)
            homePanelAmbience.Stop();

        if (gameplayBirdAmbience != null)
            gameplayBirdAmbience.Stop();
    }

    private void ShowHomeMenu()
    {
        SetPanel(homePanel, true);
        SetPanel(gameplayHUD, false);
        SetPanel(gameOverPanel, false);

        StopAllAmbience();

        if (homePanelAmbience != null)
            homePanelAmbience.Play();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        score = 0f;
        gameOver = false;
        gameStarted = true;

        StopAllAmbience();

        if (gameplayBirdAmbience != null)
            gameplayBirdAmbience.Play();

        PlayerMovement playerMovement =
            FindAnyObjectByType<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.StartRunning();
            Debug.Log("PLAYER MOVEMENT STARTED FROM GAMEMANAGER!");
        }
        else
        {
            Debug.LogError("PlayerMovement not found!");
        }

        SetPanel(homePanel, false);
        SetPanel(gameplayHUD, true);
        SetPanel(gameOverPanel, false);

        UpdateScoreText();

        Debug.Log("GAME STARTED!");
    }

    public void GameOver()
    {
        if (gameOver)
            return;

        gameOver = true;
        gameStarted = false;

        PlayerMovement playerMovement =
            FindAnyObjectByType<PlayerMovement>();

        if (playerMovement != null)
            playerMovement.StopMovement();

        StopAllAmbience();

        SetPanel(gameplayHUD, false);
        SetPanel(gameOverPanel, true);

        Time.timeScale = 0f;

        Debug.Log("GAME OVER!");
    }

    public void RestartGame()
    {
        Debug.Log("RESTART BUTTON CLICKED!");

        Time.timeScale = 1f;

        restartIntoGame = true;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();
    }
}
