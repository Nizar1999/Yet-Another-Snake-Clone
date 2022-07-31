using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static event System.Action<GameState> OnStateChanged;
    public GameState state;

    [SerializeField] GameObject[] BGBorders;
    [SerializeField] Text scoreUI;
    [SerializeField] GameObject food;

    private int score = 0;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            instance = this;
        }
    }
    private void Start()
    {
        InvokeRepeating("SpawnFood", 3f, 3f);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("Game");
    }

    private void SpawnFood()
    {
        if (state != GameState.Playing)
            return;
        float x = UnityEngine.Random.Range(BGBorders[2].transform.position.x + 0.5f, BGBorders[3].transform.position.x - 0.5f);
        float y = UnityEngine.Random.Range(BGBorders[1].transform.position.y + 0.5f, BGBorders[0].transform.position.y - 0.5f);
        Instantiate(food);
        food.transform.position = new Vector3(x, y, 0);
    }

    public void EndGame()
    {
        UpdateGameState(GameState.Ending);
        Invoke("RestartGame", 1.25f);
    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void UpdateScore()
    {
        score++;
        scoreUI.text = "Score: " + score.ToString();
    }
}

public enum GameState
{
    Playing,
    Ending,
}
