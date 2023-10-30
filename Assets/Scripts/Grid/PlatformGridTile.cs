using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGridTile : MonoBehaviour
{
    public GridObject CurrentObject { get; set; }
    public bool IsOccupied { get => CurrentObject != null; }
    public Transform Tr { get => tr; }

    private Transform tr;
    private void Awake()
    {
        tr = transform;
    }
}
