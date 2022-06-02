using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;

namespace Skills
{
    public class SkillManager : MonoBehaviour
    {
        public static readonly List<Skill> selectedSkills = new();

        private readonly Dictionary<Skill, float> activeSkillReloading = new();

        private const string skillsPath = "Skills/";
        private const string collectedSkillsName = "CollectedSkills";
        private const string collectedSkillsSeparator = ",";


        [SerializeField, MinMaxSlider(0, 120)] private Vector2Int startSkillSpwanDelay = new(10, 30);
        [SerializeField, MinMaxSlider(0, 120)] private Vector2Int nextSkillSpwanDelay = new(10, 30);
        [ShowNonSerializedField] private float timeToSpawnSkill;

        [SerializeField, Required] private GameObject newSkillPattern;

        [Button("reset collected skills")]
        private void RCS() => ResetCollectedSkills();

        public PlayerController PlayerController { get; private set; }

        public static SkillManager instance;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            PlayerController = FindObjectOfType<PlayerController>();
        }

        private void Start()
        {
            foreach (Skill skill in selectedSkills)
            {
                switch (skill.Active)
                {
                    case Skill.ActiveType.Pasive:
                        skill.Activate();
                        break;
                    case Skill.ActiveType.Active:
                        activeSkillReloading.Add(skill, 0);
                        break;
                }   
            }

            timeToSpawnSkill = Random.Range(startSkillSpwanDelay.x, startSkillSpwanDelay.y);
        }

        private void Update()
        {
            Skill[] keys = activeSkillReloading.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                Skill skill = keys[i];
                if (activeSkillReloading[skill] > 0)
                    activeSkillReloading[skill] -= Time.deltaTime;
            }

            for (int i = 0; i < 3; i++)
                if (ClickedUseSkill(i))
                    TryUseSkill(i);


            timeToSpawnSkill -= Time.deltaTime;


            bool ClickedUseSkill(int index)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + index) || Input.GetKeyDown(KeyCode.Keypad1 + index))
                    return true;

                switch (index)
                {
                    case 0:
                        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
                            return true;
                        break;
                    case 1:
                        if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
                            return true;
                        break;
                    case 2:
                        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
                            return true;
                        break;
                }

                return false;
            }

            void TryUseSkill(int index)
            {
                if (selectedSkills.Count > index)
                {
                    Skill skill = selectedSkills[index];
                    if (skill.Active == Skill.ActiveType.Active && skill.CanUse())
                    {
                        skill.Activate();

                        activeSkillReloading[skill] = skill.ReloadTime;
                    }   
                }
            }
        }

        public static void SellectSkill(Skill skill)
        {
            if (selectedSkills.Contains(skill))
                Debug.LogWarning($"{skill.name} skill is already active");
            else
                selectedSkills.Add(skill);
        }
        public static void DeselectSkill(Skill skill)
        {
            if (!selectedSkills.Contains(skill))
                Debug.LogWarning($"{skill.name} skill is not active");
            else
                selectedSkills.Remove(skill);
        }

        public static void CollectSkill(Skill skill)
        {
            if (IsSkillCollected(skill))
            {
                Debug.LogWarning("This skill was collected");
                return;
            }

            string value; ;
            if (GetCollectedSkillsNames().Length > 0)
            {
                value = PlayerPrefs.GetString(collectedSkillsName, "");
                value += collectedSkillsSeparator + skill.name;
            }
            else
                value = skill.name;

            Debug.Log($"Collected new skill {skill.name}");
            PlayerPrefs.SetString(collectedSkillsName, value);
        }

        private static string[] GetCollectedSkillsNames()
        {
            string value = PlayerPrefs.GetString(collectedSkillsName, "");
            if (value == "")
                return new string[0];
            return value.Split(collectedSkillsSeparator);
        }
        private static bool IsSkillCollected(Skill skill) => GetCollectedSkillsNames().Contains(skill.name);
        public static Skill[] GetCollectedSkills()
        {
            string[] names = GetCollectedSkillsNames();
            List<Skill> skills = new();

            for (int i = 0; i < names.Length; i++)
            {
                Skill skill = Resources.Load<Skill>(skillsPath + names[i]);
                if (skill == null)
                    Debug.LogWarning($"Dont found {names[i]} in {skillsPath} folder");
                else
                    skills.Add(skill);
            }
                
            return skills.ToArray();
        }
        public static int NumberOfCollectedSkills() => GetCollectedSkillsNames().Length;

        public static Skill[] GetAllSkills() => Resources.LoadAll<Skill>(skillsPath);
        public static Skill[] GetUnCollectedSkills()
        {
            List<Skill> skills = new();
            Skill[] collected = GetCollectedSkills();
            foreach (Skill skill in GetAllSkills())
                if (!collected.Contains(skill))
                    skills.Add(skill);
            return skills.ToArray();
        }
        public static Skill GetUnCollectedRandomSkill()
        {
            Skill[] uncollectedSkills = GetUnCollectedSkills();

            if (uncollectedSkills.Length == 0)
                return null;

            int sumRarity = 0;
            foreach (Skill skill in uncollectedSkills)
            {
                if (skill.Rarity <= 0)
                {
                    Debug.LogWarning($"{skill} has incorrect rarity");
                    return null;
                }

                sumRarity += skill.Rarity;
            }
                

            int los = Random.Range(0, sumRarity);
            foreach (Skill skill in uncollectedSkills)
                if (los <= skill.Rarity)
                    return skill;
                else
                    los -= skill.Rarity;


            Debug.LogWarning("Random didn't work");
            return null;
        }

        public float ActiveSkillReloadTime(Skill skill)
        {
            if (activeSkillReloading.TryGetValue(skill, out float value))
                return value;

            return 0;
        }
        public static int SkillIndex(Skill skill) => selectedSkills.IndexOf(skill);

        public void TryLocateSkillsOnStage(Stage stage)
        {
            if (timeToSpawnSkill > 0)
                return;

            if (stage.SkillsToSpawn <= 0)
                return;

            timeToSpawnSkill = Random.Range(nextSkillSpwanDelay.x, nextSkillSpwanDelay.y);

            Debug.Log("Spawn skills");

            int spawnedSkills = 0;
            List<Stage.SkillSpawnPoint> usedSpawns = new();
            for (int i = 0; i < 100; i++)
            {
                foreach (Stage.SkillSpawnPoint spawnPoint in stage.SkillSpawnPoints)
                {
                    if (usedSpawns.Contains(spawnPoint))
                        continue;

                    if (spawnPoint.SpawnChance >= Random.Range(0 + i, 100))
                    {
                        GameObject newSkill = Instantiate(newSkillPattern);
                        newSkill.transform.parent = stage.transform.parent;
                        newSkill.transform.position = stage.GridPosition(spawnPoint.Position);

                        usedSpawns.Add(spawnPoint);

                        spawnedSkills++;
                        if (spawnedSkills >= stage.SkillsToSpawn)
                            return;
                    }
                }
            }
            Debug.LogWarning($"Dont spawned skills for {stage.name}");
        }

        public void ResetCollectedSkills()
        {
            PlayerPrefs.SetString(collectedSkillsName, "");
        }
    }
}