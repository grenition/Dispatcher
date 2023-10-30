using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void VoidEventHandler();

public enum ObjectType
{
    hammer,
    key,
    ramp,
    spring,
    train
}

[System.Serializable]
public class InventoryUnit
{
    public GridObject gridObjectPrefab;
    public ObjectType objectType;

    public int CurrentCount
    {
        get => currentCount;
        set
        {
            currentCount = value;
            OnDataChanged?.Invoke();
        }
    }
    [SerializeField] private int currentCount = 5;

    public event VoidEventHandler OnDataChanged;
}
public class InventorySlot : MonoBehaviour, IPointerDownHandler
{
    public InventoryUnit Unit { get => unit; }

    [SerializeField] private InventoryUnit unit;
    [SerializeField] private TMP_Text countText;
    private void OnEnable()
    {
        UpdateCount();
        unit.OnDataChanged += UpdateCount;
    }
    private void OnDisable()
    {
        unit.OnDataChanged -= UpdateCount;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (unit.CurrentCount <= 0)
            return;

        unit.CurrentCount -= 1;
        Inventory.StartPlacingGridObject(unit.gridObjectPrefab);
    }
    private void UpdateCount()
    {
        countText.text = unit.CurrentCount.ToString();
    }
}
