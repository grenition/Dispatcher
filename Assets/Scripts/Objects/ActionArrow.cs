using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionArrow : MonoBehaviour
{
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private Transform arrow;
    [SerializeField] private LayerMask layerMask;

    private Transform tr;

    private void Awake()
    {
        tr = transform;
    }

    public void Update()
    {
        if(Physics.Raycast(tr.position, -tr.up, out RaycastHit hit, maxDistance, layerMask))
        {
            arrow.position = hit.point;
        }
        else
        {
            arrow.position = tr.position;
        }
    }
}
