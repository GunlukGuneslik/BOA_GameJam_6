using UnityEngine;
using UnityEngine.EventSystems;

public class Obstacle : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private Camera mainCamera;
    private Collider2D obstacleCollider;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private ObstacleData data;
    private PlacementGrid placementGrid;

    private Transform previousParent;
    private Vector3 previousPosition;

    [SerializeField] private LayerMask laneLayerMask;
    [SerializeField] private LayerMask inventoryLayerMask;

    private bool isOnDrag = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        obstacleCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        placementGrid = FindFirstObjectByType<PlacementGrid>();
    }

    private void Start()
    {
        if (data != null)
        {
            Initialize(data);
        }
    }

    public void Initialize(ObstacleData data)
    {
        this.data = data;
        spriteRenderer.sprite = data.conveyorSprite;
        transform.localScale = Vector3.one;
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

        spriteRenderer.sprite = data.obstacleSprite;

        transform.localScale = new Vector3(
            data.widthInCells * placementGrid.CellWidth,
            data.heightInLanes * placementGrid.LaneSpacing,
            1f
        );

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
            Debug.Log("Obstacle was placed on an inventory.");

            obstacleCollider.enabled = true;
            isOnDrag = false;
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
        int startLane = lane.laneIndex;

        int startColumn =
            placementGrid.WorldXToColumn(transform.position.x);

        return placementGrid.CanPlace(
            startLane,
            startColumn,
            data.widthInCells,
            data.heightInLanes
        );
    }

    private void PlaceOnLane(Lane lane)
    {
        int startLane = lane.laneIndex;

        int startColumn =
            placementGrid.WorldXToColumn(transform.position.x);

        float firstCellCenterX =
            placementGrid.ColumnToWorldX(startColumn);

        float snappedX =
            firstCellCenterX +
            ((data.widthInCells - 1) * placementGrid.CellWidth / 2f);

        float snappedY =
            lane.transform.position.y -
            ((data.heightInLanes - 1) * placementGrid.LaneSpacing / 2f);

        transform.position = new Vector3(
            snappedX,
            snappedY,
            0f
        );

        placementGrid.OccupyCells(
            startLane,
            startColumn,
            data.widthInCells,
            data.heightInLanes
        );
    }

    private void ReturnToPreviousPosition()
    {
        transform.SetParent(previousParent, true);
        transform.position = previousPosition;

        spriteRenderer.sprite = data.conveyorSprite;
        transform.localScale = Vector3.one;
    }
}