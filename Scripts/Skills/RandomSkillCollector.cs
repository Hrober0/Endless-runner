using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Skills;
using Audio;

public class RandomSkillCollector : MonoBehaviour
{
    [SerializeField] private SFXSO collectedSFX;

    private Skill selecredSkill;

    private void Start()
    {
        selecredSkill = SkillManager.GetUnCollectedRandomSkill();
        if (selecredSkill == null)
        {
            Debug.Log("No more skills!");
            gameObject.SetActive(false);
            Destroy(gameObject);
        }  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
            Collect();
    }

    private void Collect()
    {
        SkillManager.CollectSkill(selecredSkill);

        if (collectedSFX)
            AudioManager.PlaySFX(collectedSFX);
        Destroy(gameObject);
    }
}
