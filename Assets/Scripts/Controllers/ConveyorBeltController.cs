using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltController : MonoBehaviour
{
    // Pools
    private List<Obstacle> InActiveObstacles;
    private List<Obstacle> ActiveObstacles;

    [Header("References & Prefab")]
    [SerializeField] private Obstacle ObstacklePrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Obstacle placeholderObstacle;

    [Header("Belt Movement & Capacity")]
    [SerializeField] private float beltSpeed = 3f;
    [SerializeField] private float itemSpacing = 1.2f;
    [SerializeField] private int maxCapacity = 6;

    [Header("Spawning")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform endPoint;

    [Header("Spawn Progression Tuning")]
    [SerializeField] private float lowTierBias = 2.0f;
    [SerializeField] private float lateGameBoost = 2.5f;

    private Obstacle currentlyDraggedObstacle;

    private bool isBeltActive; // GamePlayController controls that activity.
    private float currentDifficultyProgress = 0f;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        isBeltActive = false;
        InActiveObstacles = new List<Obstacle>();
        ActiveObstacles = new List<Obstacle>();

        if (contentParent == null)
        {
            contentParent = transform;
        }

        // Gather any existing obstacles already placed under contentParent in the scene
        foreach (Transform child in contentParent)
        {
            if (child.TryGetComponent(out Obstacle obs) && obs != placeholderObstacle)
            {
                obs.gameObject.SetActive(false);
                InActiveObstacles.Add(obs);
            }
        }

        // Setup or Instantiate the exclusive placeholder
        if (placeholderObstacle == null)
        {
            placeholderObstacle = CreatePlaceholderFromPrefab();
        }
        else
        {
            placeholderObstacle.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Pre-warm the pool so instances are ready before Round 1 starts
        while (InActiveObstacles.Count < maxCapacity)
        {
            Obstacle newObs = CreateNewObstacleInstance();
            InActiveObstacles.Add(newObs);
        }
    }

    /// <summary>
    /// Called by GamePlayController when a round starts.
    /// </summary>
    public void ActivateBelt()
    {
        ResetBelt();
        currentDifficultyProgress = 0f;
        isBeltActive = true;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// Called by GamePlayController when a round ends.
    /// </summary>
    public void InactivateBelt()
    {
        isBeltActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void UpdateDifficulty(float progress)
    {
        currentDifficultyProgress = progress;
    }

    /// <summary>
    /// Instantiates a new Obstacle from ObstacklePrefab and prepares it for the inactive pool.
    /// </summary>
    private Obstacle CreateNewObstacleInstance()
    {
        Obstacle instance = Instantiate(ObstacklePrefab, contentParent);
        instance.gameObject.SetActive(false);
        return instance;
    }

    /// <summary>
    /// Instantiates a dedicated invisible placeholder from ObstacklePrefab.
    /// </summary>
    private Obstacle CreatePlaceholderFromPrefab()
    {
        Obstacle placeholder = Instantiate(ObstacklePrefab, contentParent);
        placeholder.name = "Placeholder_Obstacle";

        // Disable visuals and colliders so the player can't see or click the placeholder
        if (placeholder.TryGetComponent(out SpriteRenderer sr)) sr.enabled = false;
        if (placeholder.TryGetComponent(out Collider2D col)) col.enabled = false;

        placeholder.gameObject.SetActive(false);
        return placeholder;
    }

    private IEnumerator SpawnRoutine()
    {
        while (isBeltActive)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (CanSpawn())
            {
                SpawnObstacle();
            }
        }
    }

    private bool CanSpawn()
    {
        if (!isBeltActive) return false;

        if (ActiveObstacles.Count >= maxCapacity)
        {
            return false;
        }

        if (ActiveObstacles.Count > 0)
        {
            Obstacle newestObstacle = ActiveObstacles[ActiveObstacles.Count - 1];
            float distFromSpawn = Mathf.Abs(newestObstacle.transform.position.x - spawnPoint.position.x);
            if (distFromSpawn < itemSpacing)
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnObstacle()
    {
        Obstacle nextObstacle;

        if (InActiveObstacles.Count <= 0)
        {
            // Pool ran out (e.g., player placed obstacles on lanes) -> Instantiate a new one!
            nextObstacle = CreateNewObstacleInstance();
        }
        else
        {
            nextObstacle = InActiveObstacles[0];
            InActiveObstacles.RemoveAt(0);
        }

        // 1. Calculate weighted ID using currentDifficultyProgress
        int selectedId = GetWeightedObstacleId();

        // 2. Fetch the ObstacleData from DataManager and initialize
        if (DataManager.Instance != null)
        {
            ObstacleData data = DataManager.Instance.GetObstacleDataById(selectedId);
            nextObstacle.Initialize(data);
        }

        nextObstacle.transform.SetParent(contentParent, true);
        nextObstacle.transform.position = spawnPoint.position;
        nextObstacle.gameObject.SetActive(true);

        ActiveObstacles.Add(nextObstacle);
    }

    private int GetWeightedObstacleId()
    {
        int count = DataManager.Instance != null ? DataManager.Instance.GetObstacleCount() : 0;
        if (count <= 1) return 0;

        int maxUnlockedId = Mathf.Clamp(
            Mathf.CeilToInt(Mathf.Lerp(1f, count - 1, currentDifficultyProgress)),
            1,
            count - 1
        );

        float totalWeight = 0f;
        float[] weights = new float[maxUnlockedId + 1];

        for (int id = 0; id <= maxUnlockedId; id++)
        {
            float tierNormalized = (float)id / (count - 1);
            float baseWeight = Mathf.Pow(1f - tierNormalized * 0.75f, lowTierBias);
            float timeMultiplier = Mathf.Lerp(
                1f - tierNormalized,
                1f + (tierNormalized * lateGameBoost),
                currentDifficultyProgress
            );

            weights[id] = Mathf.Max(0.01f, baseWeight * timeMultiplier);
            totalWeight += weights[id];
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int id = 0; id <= maxUnlockedId; id++)
        {
            cumulative += weights[id];
            if (roll <= cumulative)
            {
                return id;
            }
        }

        return 0;
    }

    private void Update()
    {
        if (!isBeltActive) return;
        MoveAndCumulateBelt();
    }

    private void MoveAndCumulateBelt()
    {
        for (int i = 0; i < ActiveObstacles.Count; i++)
        {
            Obstacle current = ActiveObstacles[i];

            float limitX = (i == 0)
                ? endPoint.position.x
                : ActiveObstacles[i - 1].transform.position.x - itemSpacing;

            Vector3 pos = current.transform.position;

            pos.x = Mathf.MoveTowards(pos.x, limitX, beltSpeed * Time.deltaTime);
            pos.y = spawnPoint.position.y;

            current.transform.position = pos;
        }
    }

    /// <summary>
    /// Call this in Obstacle.OnBeginDrag
    /// Obstacle is not placed on a lane or inventory yet. Player is just holding it.
    /// </summary>
    public void RemoveObstacleFromBelt(Obstacle item)
    {
        if (currentlyDraggedObstacle != null && currentlyDraggedObstacle != item)
        {
            Debug.LogError("Why you have more than one Obstacles free??????????");
        }

        int index = ActiveObstacles.IndexOf(item);
        if (index < 0) return;

        currentlyDraggedObstacle = item;

        // Place the invisible placeholder in the exact position and list slot
        placeholderObstacle.transform.SetParent(contentParent, true);
        placeholderObstacle.transform.position = item.transform.position;
        placeholderObstacle.gameObject.SetActive(true);

        ActiveObstacles[index] = placeholderObstacle;
        Debug.Log("Obstacle is removed from the belt (space held by placeholder).");
    }

    /// <summary>
    /// Call this in Obstacle.OnEndDrag when placement is rejected.
    /// </summary>
    public void ReturnObstacleToPlaceholder(Obstacle item)
    {
        if (currentlyDraggedObstacle != item)
        {
            Debug.LogError("Why you have more than one Obstacles free??????????");
            return;
        }

        int index = ActiveObstacles.IndexOf(placeholderObstacle);
        if (index >= 0)
        {
            item.transform.SetParent(contentParent, true);
            item.transform.position = placeholderObstacle.transform.position;
            ActiveObstacles[index] = item;
        }

        placeholderObstacle.gameObject.SetActive(false);
        currentlyDraggedObstacle = null;
    }

    /// <summary>
    /// Call this in Obstacle.OnEndDrag when placement is confirmed.
    /// </summary>
    public void ConfirmObstaclePlacement(Obstacle item)
    {
        if (currentlyDraggedObstacle == item)
        {
            ActiveObstacles.Remove(placeholderObstacle);
            placeholderObstacle.gameObject.SetActive(false);
            currentlyDraggedObstacle = null;
        }
        else
        {
            Debug.LogError("Why you have more than one Obstacles free??????????");
            ActiveObstacles.Remove(item);
        }

        Debug.Log("Obstacle removed from belt. Gap in the conveyor belt will now close.");
    }

    /// <summary>
    /// Clears all active obstacles on the belt back into the inactive pool for a fresh round.
    /// </summary>
    public void ResetBelt()
    {
        if (currentlyDraggedObstacle != null)
        {
            currentlyDraggedObstacle.gameObject.SetActive(false);
            currentlyDraggedObstacle.transform.SetParent(contentParent, true);
            if (!InActiveObstacles.Contains(currentlyDraggedObstacle))
            {
                InActiveObstacles.Add(currentlyDraggedObstacle);
            }
            currentlyDraggedObstacle = null;
        }

        placeholderObstacle.gameObject.SetActive(false);
        ActiveObstacles.Remove(placeholderObstacle);

        while (ActiveObstacles.Count > 0)
        {
            Obstacle obs = ActiveObstacles[0];
            ActiveObstacles.RemoveAt(0);
            obs.gameObject.SetActive(false);
            obs.transform.SetParent(contentParent, true);
            InActiveObstacles.Add(obs);
        }
    }
}