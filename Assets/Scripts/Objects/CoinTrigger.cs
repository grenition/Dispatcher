using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTrigger : MonoBehaviour
{
    [SerializeField] private GridObject gridObjectBase;
    [SerializeField] private Animator animator;
    [SerializeField] private string exitTransitionTriggerName = "Exit";
    [SerializeField] private float lifetimeAfterTaking = 1f;
    private void OnEnable()
    {
        if (gridObjectBase == null)
            enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController player))
        {
            StartCoroutine(TakingCoinEnumarator());
        }
    }
    private IEnumerator TakingCoinEnumarator()
    {
        if (animator != null)
            animator.SetTrigger(exitTransitionTriggerName);
        yield return new WaitForSeconds(lifetimeAfterTaking);
        ObjectsPool.StopGridObject(gridObjectBase);
    }
}
