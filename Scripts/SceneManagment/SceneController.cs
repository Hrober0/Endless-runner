using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

namespace Gameplay
{
    public class SceneController : MonoBehaviour
    {
        [InfoBox("Unlimited for -1")]
        [SerializeField] private int targetFrameRate = 60;

        [SerializeField] private bool loadScenesInEditor = false;

        [SerializeField] private List<string> additionalScenesToLoad = new();

        void Awake()
        {
            Application.targetFrameRate = targetFrameRate;

            if (!Application.isEditor || loadScenesInEditor)
            {
                foreach (string name in additionalScenesToLoad)
                    if (name == gameObject.scene.name)
                        Debug.LogWarning("Can't load the same scene!");
                    else
                        SceneManager.LoadScene(name, LoadSceneMode.Additive);
            }
        }
    }
}