using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Train")
        {
            if (GameController.Instance.levelStarted)
            {
                GameController.Instance.Die();
            }
        }
    }
}
