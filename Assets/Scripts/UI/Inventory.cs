using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private InventorySlot[] inventorySlots;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        inventorySlots = GetComponentsInChildren<InventorySlot>();
    }

    public static void StartPlacingGridObject(ObjectType objectType)
    {
        if (GridInteractions.Instance == null)
            return;

        GridObject obj = ObjectsPool.GetNewGridObject(objectType);
        if (obj == null)
            return;
        GridInteractions.Instance.CurrentInteractableObject = obj;
    }
    public static void AddObjectToInventory(ObjectType objectType, int count = 1)
    {
        if (GridInteractions.Instance == null)
            return;

        foreach (var slot in Instance.inventorySlots)
        {
            if(slot.Unit.objectType == objectType)
            {
                slot.Unit.CurrentCount += count;
                return;
            }
        }
    }
    public static bool RemoveObjectFromInventory(ObjectType objectType, int count = 1)
    {
        if (GridInteractions.Instance == null)
            return false;

        foreach (var slot in Instance.inventorySlots)
        {
            if (slot.Unit.objectType == objectType && slot.Unit.CurrentCount >= count)
            {
                slot.Unit.CurrentCount -= count;
                return true;
            }
        }
        return false;
    }
    public static void ClearInventory()
    {
        if (GridInteractions.Instance == null)
            return;

        foreach (var slot in Instance.inventorySlots)
        {
            slot.Unit.CurrentCount = 0;
        }
    }
    public static bool IsObjectAvailable(ObjectType type)
    {
        if (GridInteractions.Instance == null)
            return false;

        foreach (var slot in Instance.inventorySlots)
        {
            if (slot.Unit.objectType == type && slot.Unit.CurrentCount > 0)
                return true;
        }
        return false;
    }
}
