using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Audio;

namespace Skills
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/SkillSO")]
    public class Skill : ScriptableObject
    {
        public enum EffectType
        {
            SlowCameraByPercent = 1,
            IncreasePlayerJumpSpeedByPercent = 5,
            IncreaseTileHealthyByPercent = 6,
            HealTilesInRange = 7,
            PlayerDash = 8,
        }

        public enum ActiveType { Pasive = 1, Active = 2 }


        [field: Header("General")]
        [field: SerializeField] public Texture2D Icon { get; private set; }
        [field: SerializeField] public string DisplayedText { get; private set; }
        [field: SerializeField, Min(1)] public int Rarity { get; private set; } = 1000;


        [field: Header("Effect")]
        [field: SerializeField] public EffectType Effect { get; private set; }
        [field: SerializeField, Min(0)] public int Value { get; private set; }

        [field: SerializeField, Min(0), ShowIf(nameof(IsDash))] public float DashTime { get; private set; } = 0.2f;


        [field: Header("Activation")]
        [field: SerializeField] public ActiveType Active { get; private set; } = ActiveType.Pasive;
        [field: AllowNesting, SerializeField, Min(0), ShowIf(nameof(IsActive))] public float ReloadTime { get; private set; }
        [field: AllowNesting, SerializeField, ShowIf(nameof(IsActive))] public SFXSO ActiveSFX { get; private set; }


        public bool CanUse()
        {
            if (Active == ActiveType.Pasive)
                return true;

            if (SkillManager.instance.ActiveSkillReloadTime(this) > 0)
                return false;

            return true;
        }

        public void Activate()
        {
            if (Active == ActiveType.Active)
            {
                if (ActiveSFX)
                    AudioManager.PlaySFX(ActiveSFX);
            }

            switch (Effect)
            {
                case EffectType.SlowCameraByPercent:
                    StageManager.Instance.SlownMoveByPercent(Value);
                    break;
                case EffectType.IncreasePlayerJumpSpeedByPercent:
                    SkillManager.instance.PlayerController.IncreeaseJumpSpeedByPercent(Value);
                    break;
                case EffectType.IncreaseTileHealthyByPercent:
                    StageManager.Instance.IncreeaseTileHealtByPercent(Value);
                    break;
                case EffectType.HealTilesInRange:
                    foreach (MapTile tile in StageManager.Instance.GetTiles(SkillManager.instance.PlayerController.transform.position, Value))
                        tile.Heal();
                    break;
                case EffectType.PlayerDash:
                    SkillManager.instance.PlayerController.MakeDash(SkillManager.instance.PlayerController.LastJumpDirection * Value, DashTime);
                    break;
                default:
                    Debug.LogWarning($"{Effect} boost is not definde");
                    break;
            }
        }


        private bool IsActive() => Active == ActiveType.Active;

        private bool IsDash() => Effect == EffectType.PlayerDash;
    }
}