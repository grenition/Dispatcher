using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float PressTime { get; private set; }
    public bool IsPressed { get; private set; }
    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
        PressTime = Time.time;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
    }
    public bool IsDoublePressed()
    {
        if (SystemInfo.deviceType == DeviceType.Handheld)
            return IsPressed && Input.touchCount == 2;
        else
            return IsPressed && (Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1));
    }
    public bool IsSinglePressed()
    {
        if (SystemInfo.deviceType == DeviceType.Handheld)
            return IsPressed && Input.touchCount == 1;
        else
            return IsPressed && !(Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1));
    }
}
