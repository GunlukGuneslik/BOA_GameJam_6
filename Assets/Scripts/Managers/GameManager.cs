using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GamePlayController gamePlayController;

    [SerializeField] private int roundNum;
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (gamePlayController == null) {
            Debug.LogError("ASSIGN THE GAME_PLAY_CONTROLLER!!!!");
            return;
        }

        gamePlayController.RoundFinished += HandleRoundFinished;

        // TODO: DELETE THIS
        StartGamePlay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetCurrentRoundNum() { 
        return roundNum;
    }

    public void HandleRoundFinished(float time)
    {
        if (roundNum % 2 == 1)
        {
            DataManager.Instance.IncreasePlayer1Score(time);
        }
        else {
            DataManager.Instance.IncreasePlayer2Score(time);
        }

        roundNum++;

        // TODO: Call the UI
    }

    /// <summary>
    /// To start game play first time with fresh scores.
    /// </summary>
    public void StartGamePlay() {
        roundNum = 1;
        DataManager.Instance.ResetPlayerData();
        gamePlayController.StartRound(roundNum);
    }
}
