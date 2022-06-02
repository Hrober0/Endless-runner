using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Viruses
{
    public class Virus : MonoBehaviour
    {
        public enum Dir { LeftToRight, RighTotLeft }

        [SerializeField] private float speed = 1;

        private Dir dir;
        private float maxDistance;

        private IEnumerator playerKiller;

        private void Awake()
        {
            enabled = false;
        }

        public void Set(Dir dir, float maxDistance)
        {
            this.dir = dir;
            this.maxDistance = maxDistance;

            enabled = true;
        }

        private void Update()
        {
            if (GameMaster.CurrPlayMode != GameMaster.PlayMode.Play)
                return;

            float ddf = speed * Time.deltaTime;

            transform.position += Vector3.right * (dir == Dir.LeftToRight ? 1 : -1) * ddf;

            maxDistance -= ddf;
            if (maxDistance < 0)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out PlayerController playerController))
            {
                playerKiller = TryHitPlayer(playerController);
                StartCoroutine(playerKiller);
            }   
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out PlayerController playerController))
            {
                if (playerKiller != null)
                    StopCoroutine(playerKiller);
            }
        }

        private IEnumerator TryHitPlayer(PlayerController player)
        {
            while (player != null && !player.CanKill)
                yield return null;

            player?.Kill();
        }
    }
}

