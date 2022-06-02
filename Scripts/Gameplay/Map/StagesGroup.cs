using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "NewStagesGroup", menuName = "Map/StageGroupSO")]
public class StagesGroup : ScriptableObject
{
    [field: Header("Occurrence")]
    [field: SerializeField] public bool HasReqiredMinSkills { get; private set; } = false;
    [field: SerializeField, Min(0), ShowIf(nameof(HasReqiredMinSkills))] public int ReqiredMinSkills { get; private set; } = 0;
    [field: SerializeField] public bool HasReqiredMaxkills { get; private set; } = false;
    [field: SerializeField, Min(1), ShowIf(nameof(HasReqiredMaxkills))] public int ReqiredMaxSkills { get; private set; } = 1;


    [Header("Stages")]
    [SerializeField, MinMaxSlider(1, 20)] private Vector2Int midStagesNumber = new(3, 8);
    [SerializeField, ValidateInput(nameof(IsStartStagesValid))] private List<Stage> startStages = new();
    [SerializeField, ValidateInput(nameof(IsMidStagesValid))] private List<Stage> midStages = new();
    [SerializeField, ValidateInput(nameof(IsEndStagesValid))] private List<Stage> endStages = new();

    public Stage RandomStartStage() => startStages[Random.Range(0, startStages.Count)];
    public Stage[] RandomMidStages()
    {
        int length = Random.Range(midStagesNumber.x, midStagesNumber.y);
        Stage[] stages = new Stage[length];

        for (int i = 0; i < length; i++)
            stages[i] = midStages[Random.Range(0, midStages.Count)];

        return stages;
    }
    public Stage RandomEndStage() => endStages[Random.Range(0, endStages.Count)];


    private bool IsStartStagesValid() => startStages.Count > 0;
    private bool IsMidStagesValid() => midStages.Count > 0;
    private bool IsEndStagesValid() => endStages.Count > 0;
}
