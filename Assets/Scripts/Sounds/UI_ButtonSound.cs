using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UI_ButtonSound : MonoBehaviour
{
    [SerializeField] private string clipName = "Button";


    private Button button;
    private void OnEnable()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayAudio);
    }
    private void PlayAudio()
    {
        SoundController.PlayAudioClip(clipName);
    }
}
