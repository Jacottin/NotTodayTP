using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private static bool skipMenuOnNextLoad;

    [Header("Interface")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Jeu")]
    [SerializeField] private PlayerController player;
    [SerializeField] private VehicleSpawner vehicleSpawner;

    private bool gameEnded;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        winPanel.SetActive(false);

        if (skipMenuOnNextLoad)
        {
            skipMenuOnNextLoad = false;
            StartGame();
        }
        else
        {
            ShowMenu();
        }
    }

    private void ShowMenu()
    {
        startPanel.SetActive(true);
        Time.timeScale = 0f;
        player.DisableControl();
        vehicleSpawner.enabled = false;
    }

    public void StartGame()
    {
        gameEnded = false;
        startPanel.SetActive(false);
        winPanel.SetActive(false);
        Time.timeScale = 1f;
        player.EnableControl();
        vehicleSpawner.enabled = true;
    }

    public void Win()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        winPanel.SetActive(true);
        player.DisableControl();
        Time.timeScale = 0f;
    }

    public void Replay()
    {
        skipMenuOnNextLoad = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}