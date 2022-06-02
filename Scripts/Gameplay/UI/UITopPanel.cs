using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Methods;

public class UITopPanel : MonoBehaviour
{
    private Label timerLabel;
    private Button pauseButton;

    private StageManager stageManager;

    private void OnEnable()
    {
        VisualElement root = UIMethods.FindRootElement(gameObject);

        timerLabel = root.Q<Label>("Time_Text");

        pauseButton = root.Q<Button>("PauseButton");
        pauseButton.RegisterCallback<ClickEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.Click));
        pauseButton.RegisterCallback<MouseEnterEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));

        pauseButton.RegisterCallback<ClickEvent>(evt =>
        {
            if (GameMaster.CurrPlayMode == GameMaster.PlayMode.Play)
                GameMaster.SetPlayMode(GameMaster.PlayMode.Pause);
            else
                GameMaster.SetPlayMode(GameMaster.PlayMode.Play);
        });
    }

    private void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
    }

    private void Update()
    {
        UpdateTimer();
    }

    private void UpdateTimer() => timerLabel.text = $"{stageManager.Timer.ToString("n2")}s";
}
