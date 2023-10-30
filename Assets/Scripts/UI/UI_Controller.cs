using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Controller : MonoBehaviour
{
    [SerializeField] private float delay = 0.5f;

    public UI_PageAnimation[] mainWindowObjects;
    public UI_PageAnimation[] gameObjects;
    public UI_PageAnimation[] dieObjects;

    public TMP_Text completedLevelsText;

    public void StartLevel()
    {
        GameController.Instance.StartLevel();
    }

    private void OnEnable()
    {
        GameController.OnLobbyOpened += OpenMenuPanel;
        GameController.OnLevelEnded += OpenMenuPanel;
        GameController.OnLevelStarted += OpenGamePanel;
        GameController.OnDie += OpenDiePanel;
    }
    private void OnDisable()
    {
        GameController.OnLobbyOpened -= OpenMenuPanel;
        GameController.OnLevelEnded -= OpenMenuPanel;
        GameController.OnLevelStarted -= OpenGamePanel;
        GameController.OnDie -= OpenDiePanel;
    }

    public void OpenGamePanel()
    {
        SetPageObjectsActive(mainWindowObjects, false);
        SetPageObjectsActive(gameObjects, true);
        SetPageObjectsActive(dieObjects, false);
    }
    public void OpenMenuPanel()
    {
        SetPageObjectsActive(mainWindowObjects, true);
        SetPageObjectsActive(gameObjects, false);
        SetPageObjectsActive(dieObjects, false);

        UpdateCompletedLevelsText();
    }
    public void OpenDiePanel()
    {
        SetPageObjectsActive(mainWindowObjects, false);
        SetPageObjectsActive(gameObjects, false);
        SetPageObjectsActive(dieObjects, true);

        SoundController.PlatAudioClip("Dead");
    }
    public void UpdateCompletedLevelsText()
    {
        completedLevelsText.text = "Пройденно уровней: " + GameController.Instance.CompletedLevels;
    }
    public void ResetLevelsProgerss()
    {
        GameController.Instance.ResetLevelsProgress();
        UpdateCompletedLevelsText();
    }
    public void Restart()
    {
        GameController.Instance.Restart();
    }

    public void SetPageObjectsActive(UI_PageAnimation[] pages, bool activeState)
    {
        foreach(var p in pages)
        {
            if (activeState)
            {
                StartCoroutine(OpenAfterTime(p));
            }
            else
            {
                p.CloseWithAnimation();
            }
        }
    }
    private IEnumerator OpenAfterTime(UI_PageAnimation page)
    {
        yield return new WaitForSeconds(delay);
        page.gameObject.SetActive(true);
    }
}
