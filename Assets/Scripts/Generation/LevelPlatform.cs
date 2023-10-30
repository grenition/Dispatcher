using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct InventoryUnitData
{
    public ObjectType objectType;
    public int count;
}

[RequireComponent(typeof(Platform))]
public class LevelPlatform : MonoBehaviour
{
    [SerializeField] private InventoryUnitData[] startInventory;
    [SerializeField] private float levelSpeed = 3f;

    [SerializeField] private float offset = 10f;
    private Platform platform;

    private bool levelStarted = false;
    private bool startable = true;

    private void Awake()
    {
        platform = GetComponent<Platform>();
    }
    private void Update()
    {
        if (!levelStarted && startable && platform.GetBeginPoint().x + offset > GridInteractions.Instance.InteractionEnd.position.x)
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
        if(levelStarted && platform.GetEndPoint().x + offset > GridInteractions.Instance.InteractionEnd.position.x)
        {
            print("level ended");
            GameController.Instance.EndLevel();
            levelStarted = false;
        }
    }
}
