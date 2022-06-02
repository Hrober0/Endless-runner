using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Gameplay.Viruses
{
    public class VirusSpawner : MonoBehaviour
    {
        [SerializeField, Required] private GameObject virusPattern;
        [SerializeField, MinMaxSlider(1, 120)] private Vector2Int spawnDelay = new(5, 10);
        [SerializeField, MinMaxSlider(1, 10)] private Vector2Int virusesPerSpawn = new(1, 3);
        [SerializeField, Min(0)] private int rowUpCameraCenterToSpawn = 5;
        [SerializeField, Min(0)] private int rowDownCameraCenterToSpawn = 3;

        private CameraController cameraController;

        private void Start()
        {
            cameraController = FindObjectOfType<CameraController>();
            StartCoroutine(VirusSender());
        }

        private IEnumerator VirusSender()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(spawnDelay.x, spawnDelay.y));

                if (GameMaster.CurrPlayMode != GameMaster.PlayMode.Play)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                int virusesToSpawn = Random.Range(virusesPerSpawn.x, virusesPerSpawn.y + 1);
                List<Vector2> spawnerPositions = new();
                for (int i = 0; i < 100; i++)
                {
                    var spawn = RandomSpawnPosition();
                    if (!spawnerPositions.Contains(spawn.position))
                    {
                        spawnerPositions.Add(spawn.position);
                        SpawnVirus(spawn.direction, spawn.position);

                        virusesToSpawn--;
                        if (virusesToSpawn <= 0)
                            break;
                    }
                }
            }
        }

        private (Virus.Dir direction, Vector2 position) RandomSpawnPosition()
        {
            Virus.Dir dir = (Virus.Dir)Random.Range(0, 2);
            Vector2 cameraSzie = cameraController.GetCameraWorldSize();
            Vector2 spawnPosition;

            // x
            const float xOffset = 2;
            if (dir == Virus.Dir.LeftToRight)
                spawnPosition.x = cameraController.transform.position.x - cameraSzie.x / 2f - xOffset;
            else
                spawnPosition.x = cameraController.transform.position.x + cameraSzie.x / 2f + xOffset;

            // y
            int yOffset = Random.Range(-rowDownCameraCenterToSpawn, rowUpCameraCenterToSpawn);
            int cameraGridY = StageManager.Instance.GridPos(cameraController.transform.position).y;
            spawnPosition.y = StageManager.Instance.WorldPos(0, cameraGridY + yOffset).y;

            return (dir, spawnPosition);
        }

        private void SpawnVirus(Virus.Dir direction, Vector2 startPosition)
        {
            // spawn
            GameObject newVirus = Instantiate(virusPattern);
            newVirus.transform.parent = transform;
            newVirus.transform.position = startPosition;
            newVirus.GetComponent<Virus>().Set(direction, cameraController.GetCameraWorldSize().x + 4);
        }
    }
}