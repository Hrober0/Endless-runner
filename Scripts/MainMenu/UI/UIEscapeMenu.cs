using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Methods;
using Audio;

namespace Gameplay.UI
{
    public class UIEscapeMenu : MonoBehaviour
    {
        private VisualElement mainButtons;

        private VisualElement mainOptions;
        private SliderInt musicSlider;
        private SliderInt sfxSlider;

        private void OnEnable()
        {
            VisualElement root = UIMethods.FindRootElement(gameObject);

            // main buttons
            mainButtons = root.Q<VisualElement>("UI_MainMenu_ButtonsList");

            Button playGameButton = mainButtons.Q<Button>("PlayGame");
            playGameButton.RegisterCallback<ClickEvent>(evt =>
            {
                switch (GameMaster.CurrPlayMode)
                {
                    case GameMaster.PlayMode.MainMenu:
                    case GameMaster.PlayMode.PlayAgain:
                        Close();
                        FindObjectOfType<UIStartingScreen>()?.Open();
                        break;
                    case GameMaster.PlayMode.Pause:
                        GameMaster.SetPlayMode(GameMaster.PlayMode.Play);
                        break;
                    default:
                        Debug.LogWarning("?");
                        break;
                }
            });
            playGameButton.RegisterCallback<ClickEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.Click));
            playGameButton.RegisterCallback<MouseEnterEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));

            Button optionsButton = mainButtons.Q<Button>("Options");
            optionsButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (UIMethods.VisualElementActive(mainOptions))
                    CloseOptions();
                else
                    OpenOptions();
            });
            optionsButton.RegisterCallback<ClickEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.Click));
            optionsButton.RegisterCallback<MouseEnterEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));

            Button exitButton = mainButtons.Q<Button>("Exit");
            exitButton.RegisterCallback<ClickEvent>(evt =>
            {
                switch (GameMaster.CurrPlayMode)
                {
                    case GameMaster.PlayMode.MainMenu:
                        Application.Quit();
                        break;
                    case GameMaster.PlayMode.Play:
                    case GameMaster.PlayMode.Pause:
                        GameMaster.SetPlayMode(GameMaster.PlayMode.MainMenu);
                        break;
                }
            });
            exitButton.RegisterCallback<ClickEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.Click));
            exitButton.RegisterCallback<MouseEnterEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));


            // options
            mainOptions = root.Q<VisualElement>("UI_MainMenu_Options");

            Button closeOptionsButton = mainOptions.Q<Button>("CloseButton");
            closeOptionsButton.RegisterCallback<ClickEvent>(evt => CloseOptions());
            closeOptionsButton.RegisterCallback<ClickEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.Click));
            closeOptionsButton.RegisterCallback<MouseEnterEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));

            musicSlider = mainOptions.Q<SliderInt>("AudioMusic");
            musicSlider.RegisterValueChangedCallback(evt => AudioManager.SetMusicVolume(evt.newValue / 100f));

            sfxSlider = mainOptions.Q<SliderInt>("AudioSFX");
            sfxSlider.RegisterValueChangedCallback(evt => AudioManager.SetSFXVolume(evt.newValue / 100f));

            GameMaster.OnPlayModeChanged += OnPlayModeChanged;

            CloseOptions();
            if (GameMaster.CurrPlayMode == GameMaster.PlayMode.MainMenu)
                Open();
            else
                Close();
        }

        private void OnDisable()
        {
            GameMaster.OnPlayModeChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(GameMaster.PlayMode playMode)
        {
            switch (playMode)
            {
                case GameMaster.PlayMode.Play:
                    Close();
                    break;
                case GameMaster.PlayMode.Pause:
                    Open();
                    break;
            }
        }


        public void Open() => UIMethods.SetActiveElement(mainButtons, true);
        public void Close()
        {
            UIMethods.SetActiveElement(mainButtons, false);
            CloseOptions();
        }

        private void OpenOptions()
        {
            musicSlider.SetValueWithoutNotify(Mathf.RoundToInt(AudioManager.MusicVolume * 100));
            sfxSlider.SetValueWithoutNotify(Mathf.RoundToInt(AudioManager.SFXVolume * 100));
            UIMethods.SetActiveElement(mainOptions, true);
        }
        private void CloseOptions() => UIMethods.SetActiveElement(mainOptions, false);
    }
}