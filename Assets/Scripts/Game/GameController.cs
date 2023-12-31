using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static VoidEventHandler OnLevelStarted;
    public static VoidEventHandler OnLevelEnded;
    public static VoidEventHandler OnLobbyOpened;
    public static VoidEventHandler OnDie;

    public float PlatformsSpeed { get => targetPlatformSpeed; set { targetPlatformSpeed = value; } }
    public int CompletedLevels { get; private set; }
    public bool levelStarted { get; private set; }

    [SerializeField] private float targetPlatformSpeed = 1f;
    [SerializeField] private float lobbyPlatformSpeed = 2f;
    [SerializeField] private float lobbyBoostedPlatformSpeed = 7f;
    [SerializeField] private float lerpingSpeedMultiplier = 15f;

    private float currentPlatformSpeed;

    private float savedSpeed = 1f;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        savedSpeed = targetPlatformSpeed;
        CompletedLevels = PlayerPrefs.GetInt("CompletedLevel");
    }
    private void Start()
    {
        OpenLobby();
    }
    private void Update()
    {
        currentPlatformSpeed = Mathf.Lerp(currentPlatformSpeed, targetPlatformSpeed, lerpingSpeedMultiplier * Time.deltaTime);
        if (Input.GetKey(KeyCode.P))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)){
                PlayerPrefs.SetInt("CompletedLevel", 0);
                Restart();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)){
                PlayerPrefs.SetInt("CompletedLevel", 1);
                Restart();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                PlayerPrefs.SetInt("CompletedLevel", 2);
                Restart();
            }
        }
    }
    public void StartLevel()
    {
        savedSpeed = targetPlatformSpeed;

        int level = PlayerPrefs.GetInt("CompletedLevel");
        CompletedLevels = level;

        if (!PlatformsPool.IsLevelAvailable(level))
            return;

        PlatformGenerator.CurrentLevelId = level;
        PlatformGenerator.GenerationType = PlatformGenerationType.level;
        targetPlatformSpeed = lobbyBoostedPlatformSpeed;
        levelStarted = true;
        OnLevelStarted?.Invoke();

        StartCoroutine(ChangeGenerationAfterTime(PlatformGenerationType.tonnels, 2f));
    }
    public void EndLevel()
    {
        int level = PlayerPrefs.GetInt("CompletedLevel");
        level++;
        CompletedLevels = level;
        PlayerPrefs.SetInt("CompletedLevel", level);

        levelStarted = false;
        OnLevelEnded?.Invoke();

        OpenLobby();
    }
    public void OpenLobby()
    {
        targetPlatformSpeed = savedSpeed;
        PlatformGenerator.GenerationType = PlatformGenerationType.tonnels;
        targetPlatformSpeed = lobbyPlatformSpeed;
        levelStarted = false;
        OnLobbyOpened?.Invoke();
    }
    public void Die()
    {
        targetPlatformSpeed = 0f;
        levelStarted = false;
        OnDie?.Invoke();
        PlayerController.Die();
    }
    public void ResetLevelsProgress()
    {
        PlayerPrefs.SetInt("CompletedLevel", 0);
        CompletedLevels = 0;
    }
    public void Restart()
    {
        SceneLoader.ReloadCurrentScene();
    }
    private IEnumerator ChangeGenerationAfterTime(PlatformGenerationType generation, float holdTime)
    {
        yield return new WaitForSecondsRealtime(holdTime);
        PlatformGenerator.GenerationType = generation;
    }
}
