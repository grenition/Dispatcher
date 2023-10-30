using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [SerializeField] private InventorySlot[] inventorySlots;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public static void StartPlacingGridObject(GridObject gridObjectPrefab)
    {
        if (gridObjectPrefab == null || GridInteractions.Instance == null)
            return;

        GridObject obj = Instantiate(gridObjectPrefab);
        gridObjectPrefab.SetInventoryObject();
        GridInteractions.Instance.CurrentInteractableObject = obj;
    }
    public static void AddObjectToInventory(ObjectType objectType, int count = 1)
    {
        foreach(var slot in Instance.inventorySlots)
        {
            if(slot.Unit.objectType == objectType)
            {
                slot.Unit.CurrentCount += count;
                return;
            }
        }
    }
    public static void ClearInventory()
    {
        foreach (var slot in Instance.inventorySlots)
        {
            slot.Unit.CurrentCount = 0;
        }
    }
}
