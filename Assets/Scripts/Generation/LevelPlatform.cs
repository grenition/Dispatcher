using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct InventoryUnitData
{
    public ObjectType objectType;
    public int count;
}
public class LevelPlatform : Platform
{
    [SerializeField] private InventoryUnitData[] startInventory;
    [SerializeField] private float levelSpeed = 3f;

    [SerializeField] private float offset = 10f;

    private bool levelStarted = false;
    private bool startable = true;

    private void Update()
    {
        if (!levelStarted && startable && GetBeginPoint().x + offset > GridInteractions.Instance.InteractionEnd.position.x)
        {
            GameController.Instance.PlatformsSpeed = levelSpeed;
            Inventory.ClearInventory();
            foreach(var j in startInventory)
            {
                Inventory.AddObjectToInventory(j.objectType, j.count);
            }
            levelStarted = true;
            startable = false;
            print("level startes");
        }
        if(levelStarted && GetEndPoint().x + offset > GridInteractions.Instance.InteractionEnd.position.x)
        {
            print("level ended");
            GameController.Instance.EndLevel();
            levelStarted = false;
        }
    }
}
