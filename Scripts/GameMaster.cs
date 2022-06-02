using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    public enum PlayMode { MainMenu, Play, Pause, GameOver, PlayAgain }

    public enum Scene { Game, MainMenu }

    public static PlayMode CurrPlayMode { get; private set; } = PlayMode.MainMenu;

    public static event Action<PlayMode> OnPlayModeChanged;


    private static GameMaster instance;
    private void Start()
    {
        instance = this;
        SetPlayMode(PlayMode.Play);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            switch (CurrPlayMode)
            {
                case PlayMode.Play:
                    SetPlayMode(PlayMode.Pause);
                    break;
                case PlayMode.Pause:
                    SetPlayMode(PlayMode.Play);
                    break;
            } 
        }  
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (CurrPlayMode)
            {
                case PlayMode.Play:
                    CurrPlayMode = PlayMode.Pause;
                    LoadScene(Scene.Game);
                    break;

                case PlayMode.Pause:
                case PlayMode.GameOver:
                    LoadScene(Scene.Game);
                    break;
            }
        }
    }

    public static void SetPlayMode(PlayMode playMode)
    {
        if (CurrPlayMode == playMode)
        {
            Debug.LogWarning($"{CurrPlayMode} mod eis already set");
            return;
        }
            

        Debug.Log($"Enter {playMode} mode");

        CurrPlayMode = playMode;

        OnPlayModeChanged?.Invoke(CurrPlayMode);

        switch (playMode)
        {
            case PlayMode.MainMenu:
                LoadScene(Scene.MainMenu);
                break;
            case PlayMode.Play:
                break;
            case PlayMode.Pause:
                break;
            case PlayMode.GameOver:
                break;
            case PlayMode.PlayAgain:
                LoadScene(Scene.MainMenu);
                break;
            default:
                break;
        }  
    }
    public static void SetPlayMode(PlayMode playMode, float delay)
    {
        if (instance == null)
        {
            Debug.LogWarning("Cant wait instance is null");
            SetPlayMode(playMode);
        }
        else
            instance.StartCoroutine(SetPlayModeDelay(playMode, delay));
    }
    private static IEnumerator SetPlayModeDelay(PlayMode playMode, float delay)
    {
        yield return new WaitForSeconds(delay);

        SetPlayMode(playMode);
    }

    public static void LoadScene(Scene scene)
    {
        switch (scene)
        {
            case Scene.Game:
                SceneManager.LoadScene("Prototype1");
                break;
            case Scene.MainMenu:
                SceneManager.LoadScene("MainMenu");
                break;
            default:
                Debug.LogWarning($"Attempted to load undeclred {scene} scene");
                break;
        }
    }
    public static void LoadScene(Scene scene, float delay)
    {
        if (instance == null)
        {
            Debug.LogWarning("Cant wait instance is null");
            LoadScene(scene);
        }
        else
            instance.StartCoroutine(LoadSceneDelay(scene, delay));
    }
    private static IEnumerator LoadSceneDelay(Scene scene, float delay)
    {
        yield return new WaitForSeconds(delay);

        LoadScene(scene);
    }
}
