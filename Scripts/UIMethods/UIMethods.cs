using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Audio;

namespace Methods
{
    public static class UIMethods
    {
        public static void SetActiveElement(VisualElement element, bool state)
        {
            if (element == null)
            {
                Debug.LogWarning("Visual element is null!");
                return;
            }

            DisplayStyle stargetDisplay = state ? DisplayStyle.Flex : DisplayStyle.None;
            if (element.style.display != stargetDisplay)
                element.style.display = stargetDisplay;
        }
        public static bool VisualElementActive(VisualElement element)
        {
            if (element == null)
            {
                Debug.LogWarning("Visual element is null!");
                return false;
            }

            return element.style.display == DisplayStyle.Flex;
        }


        public static void HideAllElementChildrens(VisualElement element)
        {
            if (element == null)
            {
                Debug.LogWarning("Visual element is null!");
                return;
            }

            for (int i = 0; i < element.childCount; i++)
                SetActiveElement(element.ElementAt(i), false);
        }


        public static VisualElement FindRootElement(GameObject gameObject)
        {
            UIDocument document = gameObject.GetComponent<UIDocument>();
            if (document == null) document = gameObject.transform.parent.GetComponent<UIDocument>();

            if (document == null)
            {
                Debug.LogError("Dont found root element for " + gameObject.name + "object");
                return null;
            }
                
            return document.rootVisualElement;
        }


        public static float CountPercent(float a, float b) => b == 0 ? 0 : Mathf.Clamp(a / b, 0f, 1f);
        public static string DisplayedPercent(float percent) => Mathf.RoundToInt(percent * 100) + "%";


        public static void SetElementClass(VisualElement element, string clas, bool value)
        {
            if (element == null)
            {
                Debug.LogWarning("Visual element is null!");
                return;
            }

            if (!value)
                element.RemoveFromClassList(clas);
            else if (!element.ClassListContains(clas))
                element.AddToClassList(clas);
        }


        private static readonly Dictionary<string, VisualTreeAsset> loadedVTAssets = new();
        public static VisualTreeAsset LoadVTAsset(string path)
        {
            if (loadedVTAssets.TryGetValue(path, out VisualTreeAsset asset))
                return asset;

            asset = UnityEngine.Resources.Load<VisualTreeAsset>(path);
            if (asset == null)
                Debug.LogWarning("Asset not found at " + path);

            loadedVTAssets.Add(path, asset);
            return asset;
        }


        public enum SFX { Click, MouseEnter }
        private static readonly Dictionary<SFX, SFXSO> loadedSFXs = new();
        public static void PlaySound(SFX type)
        {
            if (loadedSFXs.TryGetValue(type, out SFXSO sfx))
                AudioManager.PlaySFX(sfx);
            else
            {
                string path = type switch
                {
                    SFX.Click => "SFXSOs/SFX_UI_Click",
                    SFX.MouseEnter => "SFXSOs/SFX_UI_Hover",
                    _ => "undefined"
                };

                sfx = Resources.Load<SFXSO>(path);
                if (sfx == null)
                {
                    Debug.LogWarning($"Dont found {type} sfx at path: {path}");
                    return;
                }

                loadedSFXs.Add(type, sfx);
                AudioManager.PlaySFX(sfx);
            } 
        }


        private const float multiClicksResetTime = 0.4f;
        private static readonly Dictionary<MultiClickKey, (int clickCount, float time)> multiClicks = new();
        public static void RegisterMultiClick(Button button, Action callback, int clickCount = 2)
        {
            MultiClickKey key = new(button, callback, clickCount);

            button.RegisterCallback<ClickEvent>(evt => OnMultiClickButtonClicked(key));

            if (!multiClicks.ContainsKey(key))
                multiClicks.Add(key, (0, 0));
        }
        private static void OnMultiClickButtonClicked(MultiClickKey key)
        {
            float clickTime = Time.timeSinceLevelLoad;
            if (multiClicks.TryGetValue(key, out var value))
            {
                if (clickTime - value.time > multiClicksResetTime)
                    multiClicks[key] = (1, clickTime);
                else
                {
                    multiClicks[key] = (value.clickCount + 1, clickTime);
                    if (value.clickCount + 1 >= key.targetClickCount)
                        key.callback.Invoke();
                }
            }
        }
        private struct MultiClickKey
        {
            public Button button;
            public Action callback;
            public int targetClickCount;

            public MultiClickKey(Button button, Action callback, int targetClickCount)
            {
                this.button = button;
                this.callback = callback;
                this.targetClickCount = targetClickCount;
            }
        }
    }
}
