using UnityEngine;

public class PlacementGrid : MonoBehaviour
{
    [SerializeField] private float gridStartX = -10f;
    [SerializeField] private int columnCount = 20;
    [SerializeField] private int laneCount = 4;
    [SerializeField] private float cellSize = 1.5f;
    [SerializeField] private Transform topLane;
    public float CellSize => cellSize;
    private bool[,] occupiedCells;
    public float GridStartX => gridStartX;
    public int ColumnCount => columnCount;

    private void Awake()
    {
        occupiedCells = new bool[laneCount, columnCount];
    }

    public int WorldXToColumn(float worldX)
    {
        float relativeX = worldX - gridStartX;
        return Mathf.FloorToInt(relativeX / cellSize);
    }

    public float ColumnToWorldX(int column)
    {
        return gridStartX + (column * cellSize) + (cellSize / 2f);
    }

    public bool IsValidColumn(int column)
    {
        return column >= 0 && column < columnCount;
    }

    public bool CanPlace(
        int startLane,
        int startColumn,
        int widthInCells,
        int heightInLanes)
    {
        // Does it exceed the grid limits?
        if (startLane < 0 ||
            startLane + heightInLanes > laneCount ||
            startColumn < 0 ||
            startColumn + widthInCells > columnCount)
        {
            return false;
        }

        // Is any block that it is trying to fill is full?
        for (int lane = startLane;
             lane < startLane + heightInLanes;
             lane++)
        {
            for (int column = startColumn;
                 column < startColumn + widthInCells;
                 column++)
            {
                if (occupiedCells[lane, column])
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void OccupyCells(
        int startLane,
        int startColumn,
        int widthInCells,
        int heightInLanes)
    {
        for (int lane = startLane;
             lane < startLane + heightInLanes;
             lane++)
        {
            for (int column = startColumn;
                 column < startColumn + widthInCells;
                 column++)
            {
                occupiedCells[lane, column] = true;
            }
        }
    }

    public void FreeCells(
        int startLane,
        int startColumn,
        int widthInCells,
        int heightInLanes)
    {
        for (int lane = startLane;
             lane < startLane + heightInLanes;
             lane++)
        {
            for (int column = startColumn;
                 column < startColumn + widthInCells;
                 column++)
            {
                occupiedCells[lane, column] = false;
            }
        }
    }

    public int WorldXToStartColumn(float worldX, int widthInCells)
    {
        float relativeX = (worldX - gridStartX) / cellSize;

        return Mathf.RoundToInt(
            relativeX - (widthInCells / 2f)
        );
    }

    public int WorldYToStartLane(float worldY, int heightInLanes)
    {
        float gridTopY =
            topLane.position.y + (cellSize / 2f);

        float relativeY =
            (gridTopY - worldY) / cellSize;

        return Mathf.RoundToInt(
            relativeY - (heightInLanes / 2f)
        );
    }

    public Vector2 GetPlacementCenter(
        int startLane,
        int startColumn,
        int widthInCells,
        int heightInLanes)
    {
        float x =
            gridStartX +
            (startColumn * cellSize) +
            (widthInCells * cellSize / 2f);

        float gridTopY =
            topLane.position.y + (cellSize / 2f);

        float y =
            gridTopY -
            (startLane * cellSize) -
            (heightInLanes * cellSize / 2f);

        return new Vector2(x, y);
    }
}