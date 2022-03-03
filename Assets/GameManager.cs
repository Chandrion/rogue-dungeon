using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector]
    public TransitionController TransitionController;
    public AudioManager AudioManager;

    public Image DamageFlashImage;

    public static bool HasController { get => Input.GetJoystickNames().Length > 0; }

    public int LevelsUnlocked = 0;

    public bool UnlockAll = false;

    [HideInInspector]
    public static readonly string[] LEVEL_TITLES = new string[]{
        "Tutorial",
        "Spikes!",
        "Pesky Demons...",
        "Brain Transplant",
        "Party Crasher",
        "Sleepy Jailer",
        "The Arena",
        "Because Christmas"
        };

    public int LastLevelUnlocked { get => Mathf.Min(LEVEL_TITLES.Length, LevelsUnlocked + 1); }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        TransitionController = GetComponent<TransitionController>();
        AudioManager = GetComponent<AudioManager>();

        LevelsUnlocked = PlayerPrefs.GetInt("Progress", 0);

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (running && AstarPath.active)
                running = false;
            if(AstarPath.active)
                UpdatePathFinding();
        };
    }

    public void LevelCleared(string name)
    {
        if(name == $"Level {LevelsUnlocked + 1}")
        {
            LevelsUnlocked++;
            PlayerPrefs.SetInt("Progress", LevelsUnlocked);
        }
    }

    public static void UpdatePathFinding()
    {
        if (AstarPath.active)
        {
            var graphs = new NavGraph[] { AstarPath.active.data.graphs[0], AstarPath.active.data.graphs[1] };
            AstarPath.active.StartCoroutine(ScanLoopAsync(graphs));
        }
    }

    private static bool running = false;

    public static bool IsScanning => running;

    private static IEnumerator ScanLoopAsync(NavGraph[] graphs)
    {
        yield return new WaitUntil(() => !running);

        if (AstarPath.active.isScanning)
            yield break;

        running = true;

        var enumerator = AstarPath.active.ScanAsync(graphs);
        foreach (var progress in enumerator)
        {
            yield return new WaitForEndOfFrame();
        }

        running = false;
    }

    public static void DamageFlash()
    {
        if (Instance)
        {
            Instance.StopCoroutine(Instance.DamageFlashRoutine());
            Instance.StartCoroutine(Instance.DamageFlashRoutine());
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        float flashDuration = .4f;
        float flashIntensity = .5f;

        SetFlashAlpha(flashIntensity);
        for (float i = 0; i < flashDuration; i += Time.deltaTime)
        {
            SetFlashAlpha(flashIntensity * (1 - i / flashDuration));
            yield return new WaitForEndOfFrame();
        }
        SetFlashAlpha(0);
    }
    private void SetFlashAlpha(float a) => DamageFlashImage.color = new Color(DamageFlashImage.color.r, DamageFlashImage.color.g, DamageFlashImage.color.b, a);

    public bool IsUnlocked(string name)
    {
        if (UnlockAll || name.Length < 7 || name.Substring(0, 5) != "Level")
            return true;

        int level = GetLevelIndex(name);

        return (level <= LevelsUnlocked + 1);
    }

    public int GetLevelIndex(string name)
    {
        if (name.Length < 7 || name.Substring(0, 5) != "Level")
            return -1;

        int.TryParse(name.Substring(6, 1), out int level);

        return level;
    }
}
