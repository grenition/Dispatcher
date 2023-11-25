using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    public static ClickArea PlacingArea { get => Instance.placingArea; }

    [SerializeField] private ClickArea placingArea;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
}
