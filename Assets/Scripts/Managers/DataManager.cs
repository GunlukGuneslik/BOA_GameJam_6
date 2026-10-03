using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private PlayerData currentPlayerData;
    [SerializeField] private ObstacleData[] obstacleDataArray;

    // for compexity concerns.
    private Dictionary<int, ObstacleData> ObstacleById;

    [Header("Progression Tuning")]
    [Tooltip("Higher values make low-numbered obstacles much more common overall.")]
    [SerializeField] private float lowTierBias = 2.0f;

    [Tooltip("How much late-game progress boosts high-tier obstacle weights.")]
    [SerializeField] private float lateGameBoost = 2.5f;

    public static DataManager Instance;

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

        ObstacleById = new Dictionary<int, ObstacleData>();
        foreach (ObstacleData current in obstacleDataArray) {
            ObstacleById[current.obstacleId] = current;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetPlayerData();
    }

    /// <summary>
    /// To reset player scores.
    /// </summary>
    public void ResetPlayerData()
    {
        currentPlayerData.Player1Score = 0;
        currentPlayerData.Player2Score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetObstacleCount() { 
        return ObstacleById.Count;
    }

    public ObstacleData GetObstacleDataById(int id) {
        if (ObstacleById != null && ObstacleById.TryGetValue(id, out ObstacleData data)) { 
            return data;
        }

        return null;
    }

    /// <summary>
    /// Returns a weighted random ObstacleData based on round progress (0f = start, 1f = round end).
    /// Low-tier obstacles are always favored, while high-tier ones unlock and become more frequent near 1f.
    /// </summary>
    public ObstacleData GetObstacleDataForProgress(float normalizedProgress)
    {
        if (obstacleDataArray == null || obstacleDataArray.Length == 0)
        {
            Debug.LogError("ObstacleDataArray is empty in DataManager!");
            return null;
        }

        int count = obstacleDataArray.Length;
        if (count == 1) return obstacleDataArray[0];

        normalizedProgress = Mathf.Clamp01(normalizedProgress);

        // Gradually unlock higher indices as the round progresses.
        // Example with 6 obstacles: at p=0, only indices 0..1 can spawn. At p=1, indices 0..5 can spawn.
        int maxUnlockedIndex = Mathf.Clamp(
            Mathf.CeilToInt(Mathf.Lerp(1f, count - 1, normalizedProgress)),
            1,
            count - 1
        );

        float totalWeight = 0f;
        float[] weights = new float[maxUnlockedIndex + 1];

        for (int i = 0; i <= maxUnlockedIndex; i++)
        {
            // 0f = weakest obstacle, 1f = strongest obstacle in the full array
            float tierNormalized = (float)i / (count - 1);

            // 1. Base weight heavily favors low-numbered obstacles (index 0 is highest)
            float baseWeight = Mathf.Pow(1f - tierNormalized * 0.75f, lowTierBias);

            // 2. Time multiplier ramps up higher-tier weights as normalizedProgress approaches 1
            float timeMultiplier = Mathf.Lerp(
                1f - tierNormalized,
                1f + (tierNormalized * lateGameBoost),
                normalizedProgress
            );

            weights[i] = Mathf.Max(0.01f, baseWeight * timeMultiplier);
            totalWeight += weights[i];
        }

        // Standard weighted random roll
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i <= maxUnlockedIndex; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
            {
                return obstacleDataArray[i];
            }
        }

        return obstacleDataArray[0];
    }

    public void UpdateLeaderChart(int player1Score, int player2Score)
    {
        
    }

    public void IncreasePlayer1Score(float time) {
        currentPlayerData.Player1Score += time;
    }

    public void IncreasePlayer2Score(float time) { 
        currentPlayerData.Player2Score += time;
    }

}
