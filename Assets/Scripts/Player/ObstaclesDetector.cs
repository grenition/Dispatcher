using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesDetector : MonoBehaviour
{
    public float CheckingDistance { get => checkingDistance; }

    [Header("Spherecasting options")]
    [SerializeField] private float capsuleHeight = 1.5f;
    [SerializeField] private float caplsuleRadius = 0.5f;
    [SerializeField] private float checkingDistance = 3f;
    [SerializeField] private LayerMask layerMask;

    public bool CheckEmptySpace(Vector3 raycastDirection)
    {
        Vector3 p1 = transform.position - Vector3.up * (capsuleHeight * 0.5f - caplsuleRadius);
        Vector3 p2 = transform.position + Vector3.up * (capsuleHeight * 0.5f - caplsuleRadius);
        return Physics.CapsuleCast(p1, p2, caplsuleRadius, raycastDirection, checkingDistance, layerMask);
    }
}
