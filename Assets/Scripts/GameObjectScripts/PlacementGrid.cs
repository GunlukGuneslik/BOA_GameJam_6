using UnityEngine;

public class PlacementGrid : MonoBehaviour
{
    [SerializeField] private float cellWidth = 1f;
    [SerializeField] private float gridStartX = -10f;
    [SerializeField] private int columnCount = 20;
    [SerializeField] private int laneCount = 4;
    [SerializeField] private float laneSpacing = 2f;

    private bool[,] occupiedCells;
    public float CellWidth => cellWidth;
    public float GridStartX => gridStartX;
    public int ColumnCount => columnCount;
    public float LaneSpacing => laneSpacing;

    private void Awake()
    {
        occupiedCells = new bool[laneCount, columnCount];
    }

    public int WorldXToColumn(float worldX)
    {
        float relativeX = worldX - gridStartX;
        int column = Mathf.FloorToInt(relativeX / cellWidth);

        return column;
    }

    public float ColumnToWorldX(int column)
    {
        return gridStartX + (column * cellWidth) + (cellWidth / 2f);
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
}