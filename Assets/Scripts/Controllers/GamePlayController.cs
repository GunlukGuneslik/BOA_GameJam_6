using System;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayController : MonoBehaviour
{
    [Header("Obstacle Global Pool")]
    [SerializeField] private Obstacle ObstacklePrefab;
    [SerializeField] private Transform ObstaclePoolContent;
    [SerializeField] private int initialObstaclePoolCount = 15;
    private List<Obstacle> ObsteclePool; // All inactive obstacles live here

    [Header("Wisard Global Pool")]
    [SerializeField] private Wisard WisardPrefab;
    [SerializeField] private Transform WisardPoolContent;
    [SerializeField] private int initialWisardPoolCount = 15;
    private List<Wisard> WisardPool; // All inactive obstacles live here

    [Header("Controllers")]
    [SerializeField] private ConveyorBeltController conveyorBeltController;

    [Header("Difficulty Scaling (For Conveyor Belt)")]
    [Tooltip("Time in seconds when obstacle spawning reaches 100% maximum difficulty.")]
    [SerializeField] private float timeToMaxDifficulty = 90f;

    [Header("Round & Score State")]
    private int currentRound;
    [SerializeField] private float currentSurvivalTime;
    private bool isRoundActive;

    public Action<float> RoundFinished;

    public float NormalizedDifficultyProgress =>
        timeToMaxDifficulty > 0f ? Mathf.Clamp01(currentSurvivalTime / timeToMaxDifficulty) : 1f;

    private void Awake()
    {
        currentRound = -1;
        currentSurvivalTime = 0f;
        isRoundActive = false;

// For Obstacle pool
        if (ObstaclePoolContent == null)
        {
            ObstaclePoolContent = transform;
        }

        ObsteclePool = new List<Obstacle>();

        // Gather any existing obstacles already placed under PoolContent
        foreach (Transform child in ObstaclePoolContent)
        {
            if (child.TryGetComponent(out Obstacle obs))
            {
                obs.gameObject.SetActive(false);
                ObsteclePool.Add(obs);
            }
        }

        // Pre-warm the pool
        while (ObsteclePool.Count < initialObstaclePoolCount)
        {
            Obstacle newObstacle = Instantiate(ObstacklePrefab, ObstaclePoolContent);
            newObstacle.gameObject.SetActive(false);
            ObsteclePool.Add(newObstacle);
        }

// For Wisard pool

        if (WisardPoolContent == null)
        {
            WisardPoolContent = transform;
        }

        WisardPool = new List<Wisard>();

        // Gather any existing obstacles already placed under PoolContent
        foreach (Transform child in WisardPoolContent)
        {
            if (child.TryGetComponent(out Wisard wisard))
            {
                wisard.gameObject.SetActive(false);
                WisardPool.Add(wisard);
            }
        }

        // Pre-warm the pool
        while (WisardPool.Count < initialWisardPoolCount)
        {
            Wisard newWisard = Instantiate(WisardPrefab, WisardPoolContent);
            newWisard.gameObject.SetActive(false);
            WisardPool.Add(newWisard);
        }
    }

    private void OnEnable()
    {
        ConveyorBeltController.OnObstacleRequested += GetObstaclePrefab;
        ConveyorBeltController.ConveyorBeltClean += ReturnObstacleListToPool;
        Border.OnObstacleHitBoundary += ReturnObstacleToPool;
    }

    private void OnDisable()
    {
        ConveyorBeltController.OnObstacleRequested -= GetObstaclePrefab;
        ConveyorBeltController.ConveyorBeltClean -= ReturnObstacleListToPool;
        Border.OnObstacleHitBoundary -= ReturnObstacleToPool;
    }

    private void Start()
    {
        if (conveyorBeltController == null)
        {
            Debug.LogError("ASSIGN THE CONVEYOR_BELT!!!!");
        }
    }

    private void Update()
    {
        if (!isRoundActive) return;

        currentSurvivalTime += Time.deltaTime;
        conveyorBeltController.UpdateDifficulty(NormalizedDifficultyProgress);
    }

    /// <summary>
    /// Returns an inactive Obstacle from the pool (or instantiates a new one if empty).
    /// </summary>
    public Obstacle GetObstaclePrefab()
    {
        if (ObsteclePool.Count > 0)
        {
            Obstacle result = ObsteclePool[0];
            ObsteclePool.RemoveAt(0);
            return result;
        }

        Obstacle newObstacle = Instantiate(ObstacklePrefab, ObstaclePoolContent);
        newObstacle.gameObject.SetActive(false);
        return newObstacle;
    }

    /// <summary>
    /// Called via Action when an obstacle hits a Boundary, or when the belt cleans up.
    /// </summary>
    public void ReturnObstacleToPool(Obstacle obs)
    {
        if (obs == null) return;

        obs.gameObject.SetActive(false);
        obs.transform.SetParent(ObstaclePoolContent, true);

        if (!ObsteclePool.Contains(obs))
        {
            ObsteclePool.Add(obs);
        }
        else
        {
            Debug.LogError("Why this obstacle is still a child of this class?");
        }
    }

    /// <summary>
    /// Called via ConveyorBeltController.ConveyorBeltClean Action to recycle a list of obstacles.
    /// </summary>
    private void ReturnObstacleListToPool(List<Obstacle> obstacles)
    {
        if (obstacles == null) return;

        foreach (Obstacle obs in obstacles)
        {
            ReturnObstacleToPool(obs);
        }
    }

    public void StartRound(int roundNumber)
    {
        currentRound = roundNumber;
        currentSurvivalTime = 0f;
        isRoundActive = true;

        conveyorBeltController.ActivateBelt();
    }

    public void EndCurrentRound()
    {
        if (!isRoundActive) return;
        isRoundActive = false;

        conveyorBeltController.InactivateBelt();

        RoundFinished?.Invoke(currentSurvivalTime);
    }
}