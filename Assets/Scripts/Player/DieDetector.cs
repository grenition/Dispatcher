using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Train")
        {
            print("die");
            if (GameController.Instance.levelStarted)
            {
                GameController.Instance.Die();
                Player.Instance.EndGame();
            }
        }
    }
}
