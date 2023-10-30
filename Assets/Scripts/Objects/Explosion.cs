using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float radius = 10f;
    private void OnEnable()
    {
        foreach(var col in Physics.OverlapSphere(transform.position, radius))
        {
            if(col.TryGetComponent(out Player player))
            {
                if (GameController.Instance.levelStarted)
                {
                    GameController.Instance.Die();
                    Player.Instance.EndGame();
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, radius);
    }
}
