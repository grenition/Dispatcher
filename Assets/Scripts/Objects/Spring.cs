using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            player.Jump();
            if(anim != null)
            {
                anim.SetTrigger("Work");
                SoundController.PlatAudioClip("Spring");
            }
        }
    }
}
