using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleData", menuName = "Scriptable Objects/ObstacleData")]
public class ObstacleData : ScriptableObject
{
    public int obstacleId;

    public int widthInCells = 1;
    public int heightInLanes = 1;

    public Sprite conveyorSprite; // before drag
    public Sprite obstacleSprite; // during drag

    public int obstacleLevel; // in case of improvement mechanism
    //public int damageAmount; // maybe in the future
}
