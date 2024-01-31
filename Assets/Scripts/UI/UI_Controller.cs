using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Controller : MonoBehaviour
{
    [SerializeField] private float delay = 0.5f;

    [SerializeField] private UI_PageAnimation[] mainWindowObjects;
    [SerializeField] private UI_PageAnimation[] gameObjects;
    [SerializeField] private UI_PageAnimation[] dieObjects;

    [SerializeField] private TMP_Text completedLevelsText;
    [SerializeField] private TMP_Text fpsMonitor;

    [SerializeField] private Toggle coinsMovementToggle; 
    
    private IEnumerator FpsCycleEnumerator()
    {
        if (fpsMonitor == null)
            yield break;
        while (true)
        {
            fpsMonitor.text = (Mathf.RoundToInt(1f / Time.deltaTime)).ToString();
            yield return new WaitForSecondsRealtime(0.25f);
        }
    }
    public void StartLevel()
    {
        GameController.Instance.StartLevel();
    }

    private void OnEnable()
    {
        StartCoroutine(FpsCycleEnumerator());

        GameController.OnLobbyOpened += OpenMenuPanel;
        GameController.OnLevelEnded += OpenMenuPanel;
        GameController.OnLevelStarted += OpenGamePanel;
        GameController.OnDie += OpenDiePanel;

        coinsMovementToggle.onValueChanged.AddListener(SetCoinsMovementActive);
        UpdateSettingsUI();
    }
    private void OnDisable()
    {
        GameController.OnLobbyOpened -= OpenMenuPanel;
        GameController.OnLevelEnded -= OpenMenuPanel;
        GameController.OnLevelStarted -= OpenGamePanel;
        GameController.OnDie -= OpenDiePanel;

        coinsMovementToggle.onValueChanged.RemoveListener(SetCoinsMovementActive);
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

        SoundController.PlayAudioClip("Dead");
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
    public void SelectLevel(int levelId)
    {
        GameController.Instance.SetCurrentLevel(levelId);
    }
    public void SetCoinsMovementActive(bool activeState)
    {
        GameController.SetCoinsMovementActive(activeState);
    }
    public void UpdateSettingsUI()
    {
        coinsMovementToggle.isOn = GameController.Preferences.coinsMovement;
    }
    private IEnumerator OpenAfterTime(UI_PageAnimation page)
    {
        yield return new WaitForSeconds(delay);
        page.gameObject.SetActive(true);
    }
}
