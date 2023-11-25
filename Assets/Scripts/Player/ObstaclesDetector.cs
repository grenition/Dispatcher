using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesDetector : MonoBehaviour
{
    public float CheckingDistance { get => checkingDistance; }

    [Header("Spherecasting options")]
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private float capsuleHeight = 1.5f;
    [SerializeField] private float caplsuleRadius = 0.5f;
    [SerializeField] private float checkingDistance = 3f;
    [SerializeField] private LayerMask layerMask;

    public bool CheckEmptySpace(Vector3 raycastDirection)
    {
        if (raycastDirection.sqrMagnitude != 1f)
            raycastDirection.Normalize();

        Vector3 p1 = transform.position + offset - Vector3.up * (capsuleHeight * 0.5f - caplsuleRadius);
        Vector3 p2 = transform.position + offset + Vector3.up * (capsuleHeight * 0.5f - caplsuleRadius);

        raycastDirection.x = -GameController.Instance.PlatformsSpeed;
        raycastDirection.z *= PlayerController.Instance.HorizontalSpeed;
        Debug.DrawRay(transform.position + Vector3.up, raycastDirection, Color.red);

        return Physics.CapsuleCast(p1, p2, caplsuleRadius, raycastDirection, checkingDistance, layerMask);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 p1 = transform.position + offset - Vector3.up * (capsuleHeight * 0.5f - caplsuleRadius);
        Vector3 p2 = transform.position + offset + Vector3.up * (capsuleHeight * 0.5f - caplsuleRadius);
        Gizmos.DrawSphere(p1, caplsuleRadius);
        Gizmos.DrawSphere(p2, caplsuleRadius);
    }
}
