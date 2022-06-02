using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Methods;
using Skills;

namespace Gameplay.UI
{
    public class UIGameOver : MonoBehaviour
    {
        private VisualElement root;

        private Label runTimeText;

        private UIElementClassList<UISkillPanel> selectedSkills;
        private UIElementClassList<UISkillPanel> collectedSkills;

        private void OnEnable()
        {
            root = UIMethods.FindRootElement(gameObject);

            Button playAgainButton = root.Q<Button>("PlayAgainButton");
            playAgainButton.RegisterCallback<ClickEvent>(evt => GameMaster.LoadScene(GameMaster.Scene.Game));
            playAgainButton.RegisterCallback<ClickEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.Click)); 
            playAgainButton.RegisterCallback<MouseEnterEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));

            Button mainMenuButton = root.Q<Button>("MainMenuButton");
            mainMenuButton.RegisterCallback<ClickEvent>(evt => GameMaster.SetPlayMode(GameMaster.PlayMode.MainMenu));
            mainMenuButton.RegisterCallback<ClickEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.Click));
            mainMenuButton.RegisterCallback<MouseEnterEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));

            runTimeText = root.Q<Label>("YourTime");

            VisualTreeAsset skillPattern = UIMethods.LoadVTAsset("UI/UIFiles/Main menu/StartScreen/UI_MainMenu_StartScreen_Skills_Single");
            VisualElement selectedSkillsGroup = root.Q<VisualElement>("YourSkills_List");
            selectedSkills = new(skillPattern, selectedSkillsGroup);
            VisualElement collectedSkillsGroup = root.Q<VisualElement>("UI_MainMenu_StartScreen_AllSkills").Q<VisualElement>("unity-content-container");
            collectedSkills = new(skillPattern, collectedSkillsGroup);

            Close();

            GameMaster.OnPlayModeChanged += OnGameModeChanged;   
        }

        private void OnDisable()
        {
            GameMaster.OnPlayModeChanged -= OnGameModeChanged;
        }

        private void UpdateLists()
        {
            selectedSkills.ResetCounter();
            foreach (Skill skill in SkillManager.selectedSkills)
                selectedSkills.ShowElement().SetValue(skill, MoveSkillToCollectedList, true);
            selectedSkills.HideLastElements();


            collectedSkills.ResetCounter();
            foreach (Skill skill in SkillManager.GetCollectedSkills())
                if (!SkillManager.selectedSkills.Contains(skill))
                    collectedSkills.ShowElement().SetValue(skill, MoveSkillToSelectedList, false);
            collectedSkills.HideLastElements();
        }

        private void MoveSkillToSelectedList(Skill skill)
        {
            if (SkillManager.selectedSkills.Count >= 3)
                return;

            SkillManager.SellectSkill(skill);
            UpdateLists();
        }
        private void MoveSkillToCollectedList(Skill skill)
        {
            SkillManager.DeselectSkill(skill);
            UpdateLists();
        }

        private void OnGameModeChanged(GameMaster.PlayMode playMode)
        {
            if (playMode == GameMaster.PlayMode.GameOver)
                Invoke(nameof(Open), 2f);
            else
                Close();
        }

        public void Open()
        {
            UpdateLists();

            float time = StageManager.Instance != null ? StageManager.Instance.Timer : 0;
            runTimeText.text = $"Your time: {time.ToString("n2")}";

            UIMethods.SetActiveElement(root, true);
        }
        public void Close() => UIMethods.SetActiveElement(root, false);
    }
}