using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Methods;
using System;

namespace Gameplay.UI
{
    public class UIElementClassList<T> where T : UIElementClass
    {
        private readonly VisualTreeAsset pattern;
        private readonly VisualElement parent;

        private readonly List<T> list = new();
        private int counter = 0;

        public UIElementClassList(VisualTreeAsset pattern, VisualElement parent, bool hideOther = true)
        {
            this.pattern = pattern;
            this.parent = parent;

            if (hideOther)
                UIMethods.HideAllElementChildrens(parent);
        }

        /// <summary>
        /// Set active and return next element
        /// Create new if it is necessary
        /// </summary>
        public T ShowElement()
        {
            if (counter >= list.Count)
                list.Add((T)Activator.CreateInstance(typeof(T), pattern, parent));
            T element = list[counter];
            counter++;

            element.SetActive(true);
            return element;
        }

        /// <summary>
        /// Hide element that weren't showen after last counter reset
        /// </summary>
        public void HideLastElements()
        {
            for (int i = counter; i < list.Count; i++)
                list[i].SetActive(false);
        }

        public void ResetCounter() => counter = 0;
        public int Count => counter;

        public T GetElelemt(int index) => list[index];
    }
}