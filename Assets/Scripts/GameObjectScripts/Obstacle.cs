using UnityEngine;
using UnityEngine.EventSystems;

public class Obstacle : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private Camera mainCamera;
    private BoxCollider2D obstacleCollider;
    private PlacementGrid placementGrid;
    private Transform previousParent;
    private Vector3 previousPosition;

    [SerializeField] private ObstacleData data;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform visualTransform;
    [SerializeField] private LayerMask laneLayerMask;
    [SerializeField] private LayerMask inventoryLayerMask;

    private bool isOnDrag = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        obstacleCollider = GetComponent<BoxCollider2D>();
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
        if (mainCamera == null) {
            mainCamera = Camera.main;
        }

        if (obstacleCollider == null) {
            obstacleCollider = GetComponent<Collider2D>();
        }

        if (placementGrid == null) {
            placementGrid = FindFirstObjectByType<PlacementGrid>();
        }

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
        else if (previousParent.TryGetComponent(out InventoryController inventory))
        {
            Debug.Log("Obstacle was on an inventory.");
            inventory.RemoveObstacleFromInventory();
        } 
        else if (previousParent.TryGetComponent(out ConveyorBeltController belt))
        {
            Debug.Log("Obstacle was on the belt.");
            belt.RemoveObstacleFromBelt(this);
        } 
        else if (previousParent.parent != null && previousParent.parent.TryGetComponent(out ConveyorBeltController belt2)) {
            Debug.Log("Obstacle was on the belt, under a contentFolder.");
            belt2.RemoveObstacleFromBelt(this);
        } 
        else
        {
            Debug.LogError("Wrong hierarchy!!!");
            return;
        }

        ApplyObstacleVisual();

        obstacleCollider.size = new Vector2(
            data.widthInCells * placementGrid.CellSize,
            data.heightInLanes * placementGrid.CellSize
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
            if (TryPlaceObstacle())
            {
                PlaceOnGrid();

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

    private bool TryPlaceObstacle()
    {
        int startLane =
            placementGrid.WorldYToStartLane(
                transform.position.y,
                data.heightInLanes
            );

        int startColumn =
            placementGrid.WorldXToStartColumn(
                transform.position.x,
                data.widthInCells
            );

        return placementGrid.CanPlace(
            startLane,
            startColumn,
            data.widthInCells,
            data.heightInLanes
        );
    }

    private void PlaceOnGrid()
    {
        int startLane =
            placementGrid.WorldYToStartLane(
                transform.position.y,
                data.heightInLanes
            );

        int startColumn =
            placementGrid.WorldXToStartColumn(
                transform.position.x,
                data.widthInCells
            );

        Vector2 snappedPosition =
            placementGrid.GetPlacementCenter(
                startLane,
                startColumn,
                data.widthInCells,
                data.heightInLanes
            );

        transform.position = new Vector3(
            snappedPosition.x,
            snappedPosition.y,
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
        visualTransform.localScale = Vector3.one;

        obstacleCollider.size = Vector2.one; // extra security
    }

    private void ApplyObstacleVisual()
    {
        if (data == null)
        {
            Debug.LogError("ObstacleData is null.");
            return;
        }

        if (data.obstacleSprite == null)
        {
            Debug.LogError("Obstacle Sprite is null for obstacle data: " + data.name);
            return;
        }
        spriteRenderer.sprite = data.obstacleSprite;

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        float targetWidth =
            data.widthInCells * placementGrid.CellSize;

        float targetHeight =
            data.heightInLanes * placementGrid.CellSize;

        visualTransform.localScale = new Vector3(
            targetWidth / spriteSize.x,
            targetHeight / spriteSize.y,
            1f
        );
    }
}