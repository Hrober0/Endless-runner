using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;

[RequireComponent(typeof(Grid))]
public class StageManager : MonoBehaviour
{
    [field: Header("Tile size")]
    [field: SerializeField] public Vector2 TileSize { get; private set; } = Vector2.one;
    [field: SerializeField] public Vector2 TileGap { get; private set; } = Vector2.zero;



    [Header("General")]
    [SerializeField, Min(0)] private float moveSpeed = 1f;
    [SerializeField, Min(0)] private float speedAddedByStage = 0.1f;
    [SerializeField] private LayerMask tileLayer;



    [Header("Stages loading")]
    [SerializeField, MinValue(2)] private float loadStageCameraOffset = 8f;
    [SerializeField, MaxValue(-2)] private float unloaStagedCameraOffset = -8f;

    [ValidateInput(nameof(IsStartStagesListValid), "StartStages is empty!")]
    [SerializeField] private List<Stage> startStages = new();

    [ValidateInput(nameof(IsStageGroupsListValid), "StageGroups is empty or there is no group for each number of collected skills")]
    [SerializeField] private List<StagesGroup> avaStageGroups = new();

    private int counter = 0;
    private readonly List<Stage> loadedStages = new();

    private StagesGroup currentStagesGroup = null;
    private int loadedMidStagesIndex = 0;
    private Stage[] midStagesToLoad = null;



    [field: Header("Multipliers")]
    [field: SerializeField, Min(0.1f)] public float TileHealthMultiplier { get; private set; } = 1;
    public void IncreeaseTileHealtByPercent(int percent) => TileHealthMultiplier *= (100 + percent) / 100f;
    


    [field: Header("Diff")]
    [field: SerializeField, ReadOnly] public float Timer { get; private set; } = 0;

    

    private CameraController cameraController;

    private static StageManager instance;
    public static StageManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<StageManager>();
            return instance;
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GameMaster.OnPlayModeChanged += OnPlayModeChanged;
            DOTween.SetTweensCapacity(100, 400);
        }   
        else
            enabled = false;

        cameraController = FindObjectOfType<CameraController>();


        // set map
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child.gameObject.activeSelf == false)
                continue;

            Stage stage = child.GetComponent<Stage>();
            if (stage == null)
            {
                Debug.LogWarning($"Object at index {i} in {nameof(StageManager)} (object: {name}) has no {nameof(Stage)} script!");
                continue;
            }

            SetNextStage(stage);
        }

        if (loadedStages.Count == 0)
            SetNextStage(SpawnNewStage(startStages[UnityEngine.Random.Range(0, startStages.Count)]));
    }

    private void OnDestroy() => GameMaster.OnPlayModeChanged -= OnPlayModeChanged;

    private void Update()
    {
        if (GameMaster.CurrPlayMode != GameMaster.PlayMode.Play)
            return;

        MoveMap();
        Timer += Time.deltaTime;
    }


    private void OnPlayModeChanged(GameMaster.PlayMode mode)
    {
        if (mode == GameMaster.PlayMode.Play)
        {
            enabled = true;
        }
        else
        {
            enabled = false;
        }
    }

    private void MoveMap()
    {
        cameraController.MoveCamera(moveSpeed * Time.deltaTime * Vector2.up);

        if (loadedStages.Count == 0)
            Debug.LogWarning("loadedStages list is empty!");
        else
        {
            if (loadedStages[^1].TopPosition < cameraController.CameraTopPosition() + loadStageCameraOffset)
            {
                // load new stage
                LoadNextStage();
                Skills.SkillManager.instance?.TryLocateSkillsOnStage(loadedStages[^1]);
                moveSpeed += speedAddedByStage;
            }
                

            if (loadedStages[0].TopPosition < cameraController.CameraBottomPosition() + unloaStagedCameraOffset)
            {
                Destroy(loadedStages[0].gameObject);
                loadedStages.RemoveAt(0);
            }
        }
    }
    public void SlownMoveByPercent(int percent) => moveSpeed *= (100 - percent) / 100f;

    private void LoadNextStage()
    {
        Stage stageToLoad ;
        if (currentStagesGroup == null)
        {
            //Debug.Log("Loaded: start GroupStage");
            int index = UnityEngine.Random.Range(0, avaStageGroups.Count);
            int numberOfCollectedSkills = Skills.SkillManager.NumberOfCollectedSkills();
            for (int i = 0; i < avaStageGroups.Count; i++)
            {
                StagesGroup stagesGroup = avaStageGroups[index];
                if (IsStageGroupValid(stagesGroup, numberOfCollectedSkills))
                {
                    currentStagesGroup = stagesGroup;
                    break;
                }

                index++;
                if (index >= avaStageGroups.Count)
                    index = 0;
            }
            if (currentStagesGroup != null)
            {
                stageToLoad = currentStagesGroup.RandomStartStage();
                midStagesToLoad = null;
            }
            else
            {
                Debug.LogError("Loading stage: Dont found valid stage to load");
                return;
            }
        }
        else if (midStagesToLoad == null)
        {
            //Debug.Log($"Loaded: first({0}/{midStagesToLoad.Length - 1}) mid GroupStage");
            midStagesToLoad = currentStagesGroup.RandomMidStages();
            stageToLoad = midStagesToLoad[0];
            loadedMidStagesIndex = 1;
        }
        else if (loadedMidStagesIndex < midStagesToLoad.Length)
        {
            //Debug.Log($"Loaded: next({loadedMidStagesIndex} / {midStagesToLoad.Length-1}) mid GroupStage");
            stageToLoad = midStagesToLoad[loadedMidStagesIndex];
            loadedMidStagesIndex++;
        }
        else if (loadedMidStagesIndex >= midStagesToLoad.Length)
        {
            //Debug.Log("Loaded: end GroupStage");
            stageToLoad = currentStagesGroup.RandomEndStage();
            currentStagesGroup = null;
            midStagesToLoad = null;
        }
        else
        {
            Debug.LogError("Loading stage: Unexpected exception while ");
            return;
        }
        
        SetNextStage(SpawnNewStage(stageToLoad));
    }

    private void SetNextStage(Stage nextStage)
    {
        Vector2 pos;
        if (loadedStages.Count > 0)
        {
            Stage lastStage = loadedStages[^1];
            pos.y = lastStage.TopPosition + nextStage.SpaceDown;
            pos.x = lastStage.OutputPosition + (nextStage.Position.x - nextStage.InputPosition);
        }
        else
            pos = nextStage.Position;

        if (nextStage.TileGap != TileGap)
            Debug.LogWarning($"Stage {nextStage.name} has difrent {TileGap}");
        if (nextStage.TileSize != TileSize)
            Debug.LogWarning($"Stage {nextStage.name} has difrent {TileSize}");

        counter++;

        nextStage.transform.position = pos;
        nextStage.name += "-" + counter;

        loadedStages.Add(nextStage);
    }

    private Stage SpawnNewStage(Stage pattern)
    {
        GameObject stageGO = Instantiate(pattern.gameObject);
        stageGO.transform.parent = transform;
        stageGO.name = pattern.name;

        Stage stage = stageGO.GetComponent<Stage>();

        return stage;
    }


    public MapTile GetTile(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 0.1f, tileLayer.value);
        return hit.collider?.GetComponent<MapTile>();
    }
    public MapTile[] GetTiles(Vector2 midPos, int range)
    {
        Vector2 min = midPos - range * TileDistance;
        Vector2 max = midPos + range * TileDistance;
        var c = Physics2D.OverlapAreaAll(min, max, tileLayer.value);
        List<MapTile> tiles = new();
        foreach (var item in c)
            if (item.TryGetComponent(out MapTile mapTile))
                tiles.Add(mapTile);
        return tiles.ToArray();
    }

    public Vector2 TileDistance => TileSize + TileGap;

    public Vector2Int GridPos(Vector2 worldPos) => new(Mathf.RoundToInt(worldPos.x / TileDistance.x + 0.5f), Mathf.RoundToInt(worldPos.y / TileDistance.y + 0.5f));
    public Vector2 WorldPos(Vector2Int gridPos) => WorldPos(gridPos.x, gridPos.y);
    public Vector2 WorldPos(int gridX, int gridY) => new((gridX - 0.5f) * TileDistance.x, (gridY - 0.5f) * TileDistance.y);
    public Vector2 AlginPos(Vector2 worldPos) => WorldPos(GridPos(worldPos));

    public float CameraTopPosition => cameraController.CameraTopPosition();

    private bool IsStageGroupValid(StagesGroup stagesGroup, int numberOfCollectedSkills)
    {
        if (stagesGroup.HasReqiredMinSkills && numberOfCollectedSkills < stagesGroup.ReqiredMinSkills)
            return false;

        if (stagesGroup.HasReqiredMaxkills && numberOfCollectedSkills > stagesGroup.ReqiredMaxSkills)
            return false;

        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
            return;

        const float lineLength = 3;

        Vector2 upPos = (cameraController.CameraTopPosition() + loadStageCameraOffset) * Vector3.up + cameraController.transform.position.x * Vector3.right;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(upPos + lineLength * Vector2.left, upPos + lineLength * Vector2.right);

        Vector2 downPos = (cameraController.CameraBottomPosition() + unloaStagedCameraOffset) * Vector3.up + cameraController.transform.position.x * Vector3.right;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(downPos + lineLength * Vector2.left, downPos + lineLength * Vector2.right);
    }
#endif

    private bool IsStartStagesListValid() => startStages.Count > 0;
    private bool IsStageGroupsListValid()
    {
        if (avaStageGroups.Count == 0)
            return false;

        for (int i = 0; i < 20; i++)
        {
            bool found = false;

            foreach (StagesGroup sGroup in avaStageGroups)
                if (IsStageGroupValid(sGroup, i))
                {
                    found = true;
                    break;
                }

            if (found == false)
            {
                Debug.LogError($"{nameof(avaStageGroups)} list doesn't cotains group for {i} collected skkills");
                return false;
            }
        }


        return true;
    }

    private void OnValidate()
    {
        IsStageGroupsListValid();
    }
}
