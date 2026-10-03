using UnityEngine;
using UnityEngine.EventSystems;

public class Obstacle : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private Camera mainCamera;
    private Collider2D obstacleCollider;

    public Transform previousParent;
    private Vector3 previousPosition;

    private ObstacleData data;

    [SerializeField] private LayerMask laneLayerMask;
    [SerializeField] private LayerMask inventoryLayerMask;
    [SerializeField] private float gridSize = 1f;

    private bool isOnDrag = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        obstacleCollider = GetComponent<Collider2D>();
    }

    public void Initialize(ObstacleData data)
    {
        this.data = data;
    }

    /// <summary>
    /// to update Obstacle data
    /// </summary>
    public void Initialize()
    {
        this.data = new ObstacleData();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousParent = transform.parent;
        previousPosition = transform.position;

        // Şimdilik test obstacle parent olmadan da çalışabilsin.
        // Conveyor ve Inventory bağlandığında parent kontrolü aktif olarak kullanılacak.
        if (previousParent == null)
        {
            Debug.Log("Obstacke cannot be without a parent!!");
            return;
        }
        else if (previousParent.TryGetComponent(out ConveyorBeltController belt))
        {
            Debug.Log("Obstacle was on the belt.");
            belt.RemoveObstacleFromBelt(this);
        }
        else if (previousParent.TryGetComponent(out InventoryController inventory))
        {
            Debug.Log("Obstacle was on an inventory.");
            inventory.RemoveObstacleFromInventory();
        }
        else
        {
            Debug.LogError("Wrong hierarchy!!!");
            return;
        }

        //if (previousParent != null)
        //{

        //}

        isOnDrag = true;
        transform.SetParent(null, true);


        // Drag sırasında obstacle kendi collider'ına takılmasın.
        obstacleCollider.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isOnDrag) {
            return;
        }

        transform.position = ScreenToWorld2D(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 dropPosition = transform.position;

        Collider2D hitLane =
            Physics2D.OverlapPoint(dropPosition, laneLayerMask);

        if (hitLane != null)
        {
            Debug.Log("Lane detected: " + hitLane.name);
        }
        else
        {
            Debug.Log("No lane detected.");
        }

        if (hitLane != null)
        {
            Lane lane = hitLane.GetComponent<Lane>();

            if (lane != null && TryPlaceObstacle(lane))
            {
                PlaceOnLane(lane);
                obstacleCollider.enabled = true;
                isOnDrag = false;
                return;
            }
        }

        Collider2D hitInventory =
            Physics2D.OverlapPoint(dropPosition, inventoryLayerMask);

        if (hitInventory != null)
        {
            // Inventory sistemi hazır olduğunda burası doldurulacak.
            Debug.Log("Obstacle was placed on an inventory.");

            obstacleCollider.enabled = true;
            return;
        }

        ReturnToPreviousPosition();
        obstacleCollider.enabled = true;
        isOnDrag = false;
    }

    private Vector3 ScreenToWorld2D(Vector2 screenPosition)
    {
        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    -mainCamera.transform.position.z
                )
            );

        worldPosition.z = 0f;

        return worldPosition;
    }

    private bool TryPlaceObstacle(Lane lane)
    {
        // İleride:
        // - obstacle başka obstacle ile overlap ediyor mu?
        // - multi-lane obstacle sahaya sığıyor mu?
        // - yasak placement bölgesinde mi?
        // - gerekli grid cell'ler boş mu?
        //
        // kontrolleri burada yapılacak.

        return true;
    }

    private void PlaceOnLane(Lane lane)
    {
        float snappedX =
            Mathf.Round(transform.position.x / gridSize) * gridSize;

        transform.position = new Vector3(
            snappedX,
            lane.transform.position.y,
            0f
        );

        previousParent = lane.transform;
    }

    private void ReturnToPreviousPosition()
    {
        transform.SetParent(previousParent, true);
        transform.position = previousPosition;
    }
}