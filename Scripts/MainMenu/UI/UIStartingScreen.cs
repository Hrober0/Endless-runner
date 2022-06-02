using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Methods;
using Skills;

namespace Gameplay.UI
{
    public class UIStartingScreen : MonoBehaviour
    {
        private VisualElement root;

        private UIElementClassList<UISkillPanel> selectedSkills;
        private UIElementClassList<UISkillPanel> collectedSkills;

        private void OnEnable()
        {
            root = UIMethods.FindRootElement(gameObject);

            Button startGameButton = root.Q<Button>("StartGame");
            startGameButton.RegisterCallback<ClickEvent>(evt => GameMaster.LoadScene(GameMaster.Scene.Game));
            startGameButton.RegisterCallback<ClickEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.Click));
            startGameButton.RegisterCallback<MouseEnterEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));

            Button goBackButton = root.Q<Button>("GoBack");
            goBackButton.RegisterCallback<ClickEvent>(evt => { Close(); FindObjectOfType<UIEscapeMenu>()?.Open(); });
            goBackButton.RegisterCallback<ClickEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.Click));
            goBackButton.RegisterCallback<MouseEnterEvent>(evt => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));

            VisualTreeAsset skillPattern = UIMethods.LoadVTAsset("UI/UIFiles/Main menu/StartScreen/UI_MainMenu_StartScreen_Skills_Single");
            VisualElement selectedSkillsGroup = root.Q<VisualElement>("YourSkills_List");
            selectedSkills = new(skillPattern, selectedSkillsGroup);
            VisualElement collectedSkillsGroup = root.Q<VisualElement>("AllSkills_Container").Q<VisualElement>("unity-content-container");
            collectedSkills = new(skillPattern, collectedSkillsGroup);

            if (GameMaster.CurrPlayMode == GameMaster.PlayMode.PlayAgain)
                Open();
            else
                Close();
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


        public void Open()
        {
            UpdateLists();
            UIMethods.SetActiveElement(root, true);
        }
        public void Close() => UIMethods.SetActiveElement(root, false);
    }
}