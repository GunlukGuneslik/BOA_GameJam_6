using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltController : MonoBehaviour
{
    // pools
    private List<Obstacle> InActiveObstacles;
    private List<Obstacle> ActiveObstacles;

    [Header("Belt Movement & Capacity")]
    [SerializeField] private float beltSpeed = 3f;
    [SerializeField] private float itemSpacing = 1.2f;
    [SerializeField] private int maxCapacity = 6;

    [Header("Spawning")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform endPoint;

    private void Awake()
    {
        InActiveObstacles = new List<Obstacle>();
        ActiveObstacles = new List<Obstacle>();
    }

    void Start()
    {
       
    }


    void Update()
    {
        
    }

    public void RemoveObstacleFromBelt(Obstacle item)
    {
        ActiveObstacles.Remove(item);
        item.Initialize();
        InActiveObstacles.Add(item);

        Debug.Log("Obstacle is removed from the belt.");
    }
}