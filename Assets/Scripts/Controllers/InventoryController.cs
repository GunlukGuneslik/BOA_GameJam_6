using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private Obstacle currentObstacle;

    private void Awake()
    {
        currentObstacle = null;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void RemoveObstacleFromInventory() {
        if (currentObstacle == null) {
            Debug.LogError("Obstacle is not assigned to an inventory");
            return;
        }

        currentObstacle = null;
    } 
}