using System;
using UnityEngine;

public class GamePlayController : MonoBehaviour
{
    [SerializeField] private ConveyorBeltController conveyorBeltController;

    [Header("Difficulty Scaling (For Conveyor Belt)")]
    [Tooltip("Time in seconds when obstacle spawning reaches 100% maximum difficulty.")]
    [SerializeField] private float timeToMaxDifficulty = 90f;

    [Header("Round & Score State")]
    private int currentRound; // Round 1: Player 1's turn, Round 2: Player 2's turn
    [SerializeField] private float currentSurvivalTime;
    private bool isRoundActive;

    public Action<float> RoundFinished; // we need to give the survival time to score calculations.

    /// <summary>
    /// Returns a 0.0 -> 1.0 difficulty factor for ConveyorBeltController.
    /// Reaches 1.0 when currentSurvivalTime hits timeToMaxDifficulty, and stays at 1.0 beyond that.
    /// </summary>
    public float NormalizedDifficultyProgress =>
        timeToMaxDifficulty > 0f ? Mathf.Clamp01(currentSurvivalTime / timeToMaxDifficulty) : 1f;

    private void Awake()
    {
        currentRound = -1;
        currentSurvivalTime = 0f;
        isRoundActive = false;
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

        // Count up indefinitely while the runner survives
        currentSurvivalTime += Time.deltaTime;

        // Pass the updated 0.0 -> 1.0 difficulty to the belt!
        conveyorBeltController.UpdateDifficulty(NormalizedDifficultyProgress);
    }

    /// <summary>
    /// Starts or resets the timer for the given round (1 or 2).
    /// </summary>
    public void StartRound(int roundNumber)
    {
        currentRound = roundNumber;
        currentSurvivalTime = 0f;
        isRoundActive = true;

        //TODO: update the UI
        conveyorBeltController.ActivateBelt();
    }

    /// <summary>
    /// Call this function from:
    ///      PlayerController -> when player dies.
    /// </summary>
    public void EndCurrentRound()
    {
        if (!isRoundActive) return;
        isRoundActive = false;

        conveyorBeltController.InactivateBelt();

        RoundFinished?.Invoke(currentSurvivalTime);
    }
}