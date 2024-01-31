using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class ClickArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum ValueType
    {
        singlePress,
        doublePress
    }
    public Vector2 TouchPosition;
    public bool IsPressed { get; private set; }
    public bool IsDoublePressed { get; private set; }
    public bool IsSinglePressed { get; private set; }

    public Action OnUpSwipe;
    public Action OnDownSwipe;

    [SerializeField] private bool processClicks = true;
    [SerializeField] private bool processSwipes = true;
    [SerializeField] private float doubleClickMaxInterval = 0.3f;
    [SerializeField] private float partOfScreenToStartPlacingMoney = 6f;
    [SerializeField] [Range(0f, 5f)] private float minScreenSpeedToSwipe = 1f;

    private bool isWorking = false;
    private bool inTimePressed = false;
    private float savedPressTime = 0f;
    private Vector2 savedPressPosition;
    private IEnumerator TouchEnumerator()
    {
        isWorking = true;
        inTimePressed = false;
        float targetTime = Time.time + doubleClickMaxInterval;
        float partOfScreen = Screen.width / partOfScreenToStartPlacingMoney;
        float savedHorizontalPos = TouchPosition.x;
        bool objectUnderCursor = GridInteractions.CheckGridObjectsUnderCursor();
        while(Time.time < targetTime)
        {
            if (inTimePressed) 
            {
                IsSinglePressed = false;
                IsDoublePressed = true;
                StartCoroutine(ValueLifeCycleEnumerator(ValueType.doublePress));
                isWorking = false;
                yield break;
            }
            inTimePressed = false;
            if (Mathf.Abs(savedHorizontalPos - TouchPosition.x) > partOfScreen || !objectUnderCursor)
                break;
            yield return null;
        }
        IsDoublePressed = false;
        IsSinglePressed = true;
        StartCoroutine(ValueLifeCycleEnumerator(ValueType.singlePress));
        isWorking = false;
    }
    private IEnumerator ValueLifeCycleEnumerator(ValueType type)
    {
        yield return null;
        yield return new WaitUntil(() => IsPressed == false);
        switch (type)
        {
            case ValueType.singlePress:
                IsSinglePressed = false;
                yield break;
            case ValueType.doublePress:
                IsDoublePressed = false;
                yield break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        savedPressTime = Time.time;
        TouchPosition = eventData.pointerCurrentRaycast.screenPosition;
        savedPressPosition = TouchPosition;
        IsPressed = true;
        if (processClicks)
        {
            inTimePressed = true;
            if (isWorking == false)
            {
                StopAllCoroutines();
                StartCoroutine(TouchEnumerator());
            }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        TouchPosition = eventData.pointerCurrentRaycast.screenPosition;
        IsPressed = false;

        if (processSwipes)
        {
            Vector2 speed = (TouchPosition - savedPressPosition) / ((Time.time - savedPressTime) * Screen.height);
            if(speed.magnitude > minScreenSpeedToSwipe)
            {
                if (speed.y > 0)
                    OnUpSwipe?.Invoke();
                else
                    OnDownSwipe?.Invoke();
            }
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        TouchPosition = eventData.pointerCurrentRaycast.screenPosition;
    }

    public void SetActive(bool activeState)
    {
        gameObject.SetActive(activeState);
    }
}
