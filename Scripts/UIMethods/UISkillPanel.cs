using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Methods;
using Skills;

namespace Gameplay.UI
{
    public class UISkillPanel : UIElementClass
    {
        private readonly VisualElement skillIcon;
        private readonly Label skillsDescription;

        private readonly Button skillAction;

        private Skill skill;
        private Action<Skill> onClickAction;
        private bool isSelected;

        public UISkillPanel(VisualTreeAsset pattern, VisualElement parent) : base(pattern, parent)
        {
            skillIcon = main.Q<VisualElement>("YourSkills_Icon");
            skillsDescription = main.Q<Label>("YourSkills_Description");
            skillAction = main.Q<Button>("YourSkills_Action");

            skillAction.RegisterCallback<ClickEvent>(ect => onClickAction?.Invoke(skill));
            skillAction.RegisterCallback<ClickEvent>(ect => UIMethods.PlaySound(UIMethods.SFX.Click));

            main.RegisterCallback<MouseEnterEvent>(evt => SetSelected(true));
            main.RegisterCallback<MouseLeaveEvent>(evt => SetSelected(false));
            main.RegisterCallback<MouseEnterEvent>(ect => UIMethods.PlaySound(UIMethods.SFX.MouseEnter));
        }

        public void SetValue(Skill skill, Action<Skill> onClickAction, bool isSelected)
        {
            skillIcon.style.backgroundImage = skill.Icon;
            skillsDescription.text = skill.DisplayedText;

            this.skill = skill;
            this.onClickAction = onClickAction;
            this.isSelected = isSelected;
        }

        private void SetSelected(bool value)
        {
            UIMethods.SetElementClass(skillAction, "YourSkills_AddIcon", onClickAction != null && value && !isSelected);
            UIMethods.SetElementClass(skillAction, "YourSkills_Action_hover", onClickAction != null && value);
        }
    }
}
