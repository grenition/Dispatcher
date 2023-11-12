using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void VoidEventHandler();

[System.Serializable]
public class InventoryUnit
{
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
        Inventory.StartPlacingGridObject(unit.objectType);
    }
    private void UpdateCount()
    {
        countText.text = unit.CurrentCount.ToString();
    }
}
