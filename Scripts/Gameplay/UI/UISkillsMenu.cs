using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Methods;
using Skills;

namespace Gameplay.UI
{
    public class UISkillsMenu : MonoBehaviour
    {
        private UIElementClassList<SkillPanel> skillsList;

        private void OnEnable()
        {
            VisualElement root = UIMethods.FindRootElement(gameObject);

            VisualElement skillGroup = root.Q<VisualElement>("SkillsMenu");
            VisualTreeAsset skillPattern = UIMethods.LoadVTAsset("UI/UIFiles/Gameplay/UI_Gameplay_SkillSingle");
            skillsList = new(skillPattern, skillGroup);
        }

        private void Start()
        {
            InvokeRepeating(nameof(UpdateSkills), 0.1f, 0.1f);
        }

        private void UpdateSkills()
        {
            skillsList.ResetCounter();
            foreach (Skill skill in SkillManager.selectedSkills)
                skillsList.ShowElement().SetValue(skill);
            skillsList.HideLastElements();
        }


        private class SkillPanel : UIElementClass
        {
            private readonly VisualElement skillIcon;
            private readonly VisualElement timeIcon;
            private readonly Label infoText;

            private Skill lastSkill = null;

            public SkillPanel(VisualTreeAsset pattern, VisualElement parent) : base(pattern, parent)
            {
                skillIcon = main.Q<VisualElement>("SkillSingle_Icon");
                timeIcon = main.Q<VisualElement>("SkillSingle_TimeIcon");
                infoText = main.Q<Label>("SkillSingle_InfoText");
            }

            public void SetValue(Skill skill)
            {
                if (lastSkill != skill)
                {
                    lastSkill = skill;
                    skillIcon.style.backgroundImage = skill.Icon;

                    if (skill.Active == Skill.ActiveType.Pasive)
                    {
                        infoText.text = "Pasive";
                        UIMethods.SetActiveElement(timeIcon, false);
                    }     
                }

                if (skill.Active == Skill.ActiveType.Active)
                {
                    float reladTime = SkillManager.instance.ActiveSkillReloadTime(skill);
                    if (reladTime > 0)
                    {
                        infoText.text = $"{reladTime.ToString("n1")} s";
                        UIMethods.SetActiveElement(timeIcon, true);
                    }
                    else
                    {
                        string key = SkillManager.SkillIndex(skill) switch
                        {
                            0 => "8",
                            1 => "9",
                            2 => "0",
                            _ => "?"
                        };
                        infoText.text = $"Press {key}";
                        UIMethods.SetActiveElement(timeIcon, false);
                    }
                }
            }
        }
    }
}