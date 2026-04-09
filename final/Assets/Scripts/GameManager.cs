using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("<color=green>MISSION COMPLETE</color>: You escaped the labyrinth!");
        // Logic for loading next level or showing UI
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}