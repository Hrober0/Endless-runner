using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Skills
{
    public class GivenSkillCollector : MonoBehaviour
    {
        [SerializeField, Expandable, Required] private Skill skill;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<PlayerController>() != null)
                Collect();
        }

        private void Collect()
        {
            SkillManager.CollectSkill(skill);
            Destroy(gameObject);
        }
    }
}