using UnityEngine;

public class Tower : EntityBase
{
    public int gridX, gridY;
    public Vector2Int size = new Vector2Int(2, 2);
    protected override void OnIntervalUpdate(float deltaTime)
    {
        // base.(deltaTime);
    }
}
public class Soldier: EntityBase
{
    public bool isBusy;
}