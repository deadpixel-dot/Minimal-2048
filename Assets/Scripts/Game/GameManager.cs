using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TileBoard board;
    [SerializeField] private CanvasGroup gameOver;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hiscoreText;
    [SerializeField] private CanvasGroup gameWin;
    public int boardSize = 4; // Adjust according to your game
    public GameObject undoButton; // Reference to the Undo button in UI


    private int[,] gameBoard;
    public Stack<GameState> undoStack = new Stack<GameState>();
    private int score = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        NewGame();
        

    }

    private void InitializeGame()
    {
        gameBoard = new int[boardSize, boardSize];
        SaveState(); // Save initial state
    }

    public void NewGame()
    {
        SetScore(0);
        hiscoreText.text = LoadHiscore().ToString();

        gameOver.alpha = 0f;
        gameOver.interactable = false;

        gameWin.alpha = 0f;
        gameWin.interactable=false;

        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
        InitializeGame();
        SaveState(); // Save the initial game state
    }
    public void Continue()
    {
        gameWin.alpha = 0f;
        gameWin.interactable = false;
        board.enabled = true;
    }

    public void SaveState()
    {
        int[,] boardState = board.GetBoardState(); // Assume GetBoardState() returns the current board state
        GameState currentState = new GameState(boardState, score);
        undoStack.Push(currentState);
        if (undoStack.Count < 10) // Limit to 20 undo steps
        {
            undoStack = new Stack<GameState>(undoStack.Take(10));
            undoStack.Push(currentState);
            if( undoStack.Count >10)
            {
                undoStack.Clear();
            }
        }
        UpdateUndoButton();
    }

    public void Undo()
    {
        if (undoStack.Count > 1) // Keep at least the initial state
        {
            undoStack.Pop();// Remove current state
            undoStack.Pop();
            GameState previousState = undoStack.Peek();
            score = previousState.score;
            board.SetBoardState(previousState.board);
            UpdateUI();
            UpdateUndoButton();
        }
    }

    

    private void UpdateUndoButton()
    {
        if (undoButton != null)
        {
            var button = undoButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.interactable = undoStack.Count > 0;
            }
        }
    }

    private void UpdateUI()
    {
        scoreText.text = score.ToString();
    }

    public void GameOver()
    {
        board.enabled = false;
        gameOver.interactable = true;

        StartCoroutine(Fade(gameOver, 1f, 1f));
    }
    public void GameWin()
    {
        board.enabled = false;
        gameWin.interactable = true;

        StartCoroutine(Fade(gameWin, 1f, 1f));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        SaveHiscore();
    }

    private void SaveHiscore()
    {
        int hiscore = LoadHiscore();

        if (score > hiscore)
        {
            PlayerPrefs.SetInt("hiscore", score);
        }
    }

    public int LoadHiscore()
    {
        return PlayerPrefs.GetInt("hiscore", 0);
    }
    
}
