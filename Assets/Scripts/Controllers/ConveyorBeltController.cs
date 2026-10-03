using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltController : MonoBehaviour
{
    // pools
    private List<Obstacle> InActiveObstacles;
    private List<Obstacle> ActiveObstacles;

    private Obstacle placeholderObstacle;

    [SerializeField] private Transform contentParent;


    [Header("Belt Movement & Capacity")]
    [SerializeField] private float beltSpeed = 3f;
    [SerializeField] private float itemSpacing = 1.2f;
    [SerializeField] private int maxCapacity = 6;

    [Header("Spawning")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform endPoint;

    private Obstacle currentlyDraggedObstacle;

    private void Awake()
    {
        InActiveObstacles = new List<Obstacle>();
        ActiveObstacles = new List<Obstacle>();

        if (contentParent == null)
        {
            contentParent = transform;
        }

        foreach (Transform child in contentParent)
        {
            if (child.TryGetComponent(out Obstacle obs) && obs != placeholderObstacle)
            {
                obs.gameObject.SetActive(false);
                InActiveObstacles.Add(obs);
            }
        }

        // Ensure the exclusive placeholder starts hidden
        if (placeholderObstacle != null)
        {
            placeholderObstacle.gameObject.SetActive(false);
        }
        else {
            placeholderObstacle = new Obstacle();
        }
    }

    void Start()
    {
        //TODO: ALSO CLEAN THE OBSTACKLE DATA.
        // clean the pool.
        while (ActiveObstacles.Count > 0) {
            InActiveObstacles.Add(ActiveObstacles[0]);
            ActiveObstacles.Remove(ActiveObstacles[0]);
        }

        while (InActiveObstacles.Count < maxCapacity) {
            InActiveObstacles.Add(new Obstacle());
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
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
        Obstacle nextObstacle = null;
        if (InActiveObstacles == null || InActiveObstacles.Count <= 0)
        {
            nextObstacle = new Obstacle();
        }
        else {
            nextObstacle = InActiveObstacles[0];
            InActiveObstacles.RemoveAt(0);
        }

        nextObstacle.transform.SetParent(contentParent, true);
        nextObstacle.transform.position = spawnPoint.position;
        nextObstacle.Initialize();
        nextObstacle.gameObject.SetActive(true);

        ActiveObstacles.Add(nextObstacle);
    }

    void Update()
    {
        MoveAndCumulateBelt();
    }

    private void MoveAndCumulateBelt()
    {
        // Index 0 is the oldest item (closest to the right / endPoint).
        for (int i = 0; i < ActiveObstacles.Count; i++)
        {
            Obstacle current = ActiveObstacles[i];

            // Determine the furthest X position this obstacle is allowed to reach:
            // - The first item (i == 0) stops at endPoint.position.x
            // - Every item behind it (i > 0) stops at (itemAhead.x - itemSpacing), causing accumulation!
            float limitX = (i == 0)
                ? endPoint.position.x
                : ActiveObstacles[i - 1].transform.position.x - itemSpacing;

            Vector3 pos = current.transform.position;

            // Move right towards limitX, but never pass it
            pos.x = Mathf.MoveTowards(pos.x, limitX, beltSpeed * Time.deltaTime);
            pos.y = spawnPoint.position.y;

            current.transform.position = pos;
        }
    }

    /// <summary>
    /// Call this in Obstackle.BeginDrag
    /// Obstackle did not placed on a lane or inventory yet. Player Just hold it.
    /// </summary>
    /// <param name="item"></param>
    public void RemoveObstacleFromBelt(Obstacle item)
    {
        if (currentlyDraggedObstacle != item)
        {
            Debug.LogError("Why you have more than one Obstackles free??????????");
        }

        int index = ActiveObstacles.IndexOf(item);
        if (index < 0) return;

        currentlyDraggedObstacle = item;

        // Place the invisible placeholder in the exact position and list slot
        placeholderObstacle.transform.SetParent(contentParent, true);
        placeholderObstacle.transform.position = item.transform.position;
        placeholderObstacle.gameObject.SetActive(true);

        ActiveObstacles[index] = placeholderObstacle;
        Debug.Log("Obstacle is removed from the belt.");
    }

    /// <summary>
    /// Call this in Obstackle.EndDrag when placement is rejected
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void ReturnObstacleToPlaceholder(Obstacle item)
    {
        if (currentlyDraggedObstacle != item)
        {
            Debug.LogError("Why you have more than one Obstackles free??????????");
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
    /// Call this in Obstackle.EndDrag when placement is confirmed.
    /// </summary>
    /// <param name="item"></param>
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
            Debug.LogError("Why you have more than one Obstackles free??????????");
            ActiveObstacles.Remove(item);
        }

        Debug.Log("Obstacle removed from belt. Gap in the conveyor belt will now close.");
    }
}