using UnityEngine;
using UnityEngine.UIElements;
using Methods;

namespace Gameplay.UI
{
    public abstract class UIElementClass
    {
        protected readonly VisualElement main;

        public UIElementClass(VisualTreeAsset pattern, VisualElement parent)
        {
            if (pattern == null)
            {
                Debug.LogError($"Cant create new {nameof(UIElementClass)} {nameof(pattern)} is null");
                return;
            }

            if (parent == null)
            {
                Debug.LogError($"Cant create new {nameof(UIElementClass)} {nameof(parent)} is null");
                return;
            }

            TemplateContainer template = pattern.CloneTree();
            main = template;
            parent.Add(template);
            Init();
        }

        public UIElementClass(VisualElement main)
        {
            this.main = main;
            Init();
        }

        protected virtual void Init() {}

        public virtual void SetActive(bool value) => UIMethods.SetActiveElement(main, value);
        public VisualElement Main => main;
    }
}
