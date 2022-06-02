using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Stage : MonoBehaviour
{
    [Header("Stage size")]
    [SerializeField] private float downOffset = 0.5f;
    [SerializeField] private int tilesLeft = 1;
    [SerializeField] private int tilesRight = 1;
    [SerializeField, Min(1)] private int tilesColumn = 2;
    

    [Header("In and Out")]
    [SerializeField] private int inPosition = 0;
    [SerializeField] private int outPosition = 0;


    [Header("Skills")]
    [ValidateInput(nameof(IsSkillsToSpawnValid), "skillSpawnPoints count has to be grather or less then skillsToSpawn")]
    [SerializeField, Min(0)] private int skillsToSpawn = 0;
    [SerializeField] private List<SkillSpawnPoint> skillSpawnPoints = new();
    public int SkillsToSpawn => skillsToSpawn;
    public IReadOnlyList<SkillSpawnPoint> SkillSpawnPoints => skillSpawnPoints;
    private bool IsSkillsToSpawnValid() => skillsToSpawn <= skillSpawnPoints.Count;


    [field: Header("Tile size")]
    [field: InfoBox("Should be the same as in StageManager")]
    [field: SerializeField] public Vector2 TileSize { get; private set; } = Vector2.one;
    [field: SerializeField] public Vector2 TileGap { get; private set; } = Vector2.zero;

    [Button("Get values from StageManager")]
    private void GetValuesFromStageManager()
    {
        if (StageManager.Instance == null)
        {
            Debug.LogError("Missing StageManager");
            return;
        }

        TileSize = StageManager.Instance.TileSize;
        TileGap = StageManager.Instance.TileGap;
    }

    public Vector2 Position => transform.position;

    public float SpaceDown => downOffset;
    public float TopPosition => transform.position.y - downOffset + tilesColumn * TileSizeY;

    public float InputPosition => transform.position.x + (inPosition + 0.5f) * TileSizeX;
    public float OutputPosition => transform.position.x + (outPosition + 0.5f) * TileSizeX;

    private float TileSizeY => TileSize.y + TileGap.y;
    private float TileSizeX => TileSize.x + TileGap.x;

    public Vector2 GridPosition(Vector2Int gridPos) => new (Position.x + (gridPos.x + 0.5f) * TileSizeX, Position.y + (gridPos.y + 0.5f) * TileSizeY);


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // border
        Vector2 pos = transform.position;
        Vector2 size = new((tilesLeft + tilesRight) * TileSizeX, tilesColumn * TileSizeY);
        Vector2 min = pos + new Vector2(-tilesLeft * TileSizeX, -downOffset);
        Vector2 max = min + size;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(min, min + size.x * Vector2.right);
        Gizmos.DrawLine(min, min + size.y * Vector2.up);
        Gizmos.DrawLine(max, max - size.x * Vector2.right);
        Gizmos.DrawLine(max, max - size.y * Vector2.up);


        // center
        const float centerSize = 0.2f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos + centerSize * Vector2.down, pos + centerSize * Vector2.up);
        Gizmos.DrawLine(pos + centerSize * Vector2.left, pos + centerSize * Vector2.right);
        

        // in
        const float arrowSize = 0.5f;
        Vector2 inPos = new(InputPosition, min.y + arrowSize);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(inPos, inPos + arrowSize * new Vector2(1, -1));
        Gizmos.DrawLine(inPos, inPos + arrowSize * new Vector2(-1, -1));

        // out
        Vector2 outPos = new(OutputPosition, max.y);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(outPos, outPos + arrowSize * new Vector2(1, -1));
        Gizmos.DrawLine(outPos, outPos + arrowSize * new Vector2(-1, -1));


        // skills
        const float skillSize = 0.3f;
        Gizmos.color = Color.yellow;
        foreach (SkillSpawnPoint skill in skillSpawnPoints)
        {
            Vector2 skillPos = GridPosition(skill.Position);
            Gizmos.DrawLine(skillPos + skillSize * Vector2.down, skillPos + skillSize * Vector2.up);
            Gizmos.DrawLine(skillPos + skillSize * Vector2.left, skillPos + skillSize * Vector2.right);
        }
    }
#endif

    [System.Serializable]
    public class SkillSpawnPoint
    {
        [SerializeField] private Vector2Int position = Vector2Int.zero;
        [AllowNesting, SerializeField, Range(1, 100)] private int spawnChance = 50;

        public Vector2Int Position => position;
        public int SpawnChance => spawnChance;
    }
}
