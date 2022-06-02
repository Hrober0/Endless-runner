using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class MapTile : MonoBehaviour
{
    [field: SerializeField] public bool IsVirtual { get; private set; } = false;

    [field: SerializeField, Range(0, 3)] public int Height { get; private set; } = 0;

    [field: SerializeField, Min(0)] public float HP { get; private set; } = 10;
    [SerializeField] private bool lossingHP = false;
    [SerializeField, ShowIf(nameof(lossingHP))] private float lossingHPPerSecond = 1f;
    [SerializeField] private bool lossingHPIsPlayerOver = false;
    [SerializeField, ShowIf(nameof(lossingHPIsPlayerOver))] private float lossingHPIfPlayerOverPerSecond = 5f;

    [field: SerializeField, ReadOnly] public bool IsPlayerOver { get; private set; } = false;
    [field: SerializeField, ReadOnly] public bool IsInCameraView { get; private set; } = false;
    [field: SerializeField, ReadOnly] public bool IsDestroyed { get; private set; } = false;


    private float defaultHP;
    private MaterialPropertyBlock materialPropertyBlock;
    private SpriteRenderer spriteRenderer;

    private Sequence virtualShake;

    private void Start()
    {
        HP *= StageManager.Instance.TileHealthMultiplier;
        defaultHP = HP;
        materialPropertyBlock = new();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (IsVirtual)
        {
            const float def = 0.1f;
            const float t = 0.07f;
            Vector3 localPos = transform.localPosition;
            virtualShake = DOTween.Sequence()
                .AppendInterval(Random.Range(1f, 3f))
                .Append(transform.DOLocalMove(localPos + new Vector3(Random.Range(def, def), Random.Range(def, def), 0), Random.Range(t, t * 2)))
                .Append(transform.DOLocalMove(localPos, Random.Range(t, t * 4)))
                .Append(transform.DOLocalMove(localPos + new Vector3(Random.Range(-def, def), Random.Range(-def, def), 0), Random.Range(t, t * 2)))
                .Append(transform.DOLocalMove(localPos, Random.Range(t, t * 4)))
                .SetLoops(-1)
                ;
        }
    }

    private void Update()
    {
        if (GameMaster.CurrPlayMode != GameMaster.PlayMode.Play)
            return;

        if (IsInCameraView == false)
            IsInCameraView = transform.position.y <= StageManager.Instance.CameraTopPosition;


        if (lossingHP && IsInCameraView)
            HP -= lossingHPPerSecond * Time.deltaTime;


        if (lossingHPIsPlayerOver && IsPlayerOver)
            HP -= lossingHPIfPlayerOverPerSecond * Time.deltaTime;


        if (lossingHP || lossingHPIsPlayerOver && IsPlayerOver)
        {
            // updat visual
            float tv = 1 - HP / defaultHP;
            float value = 1 - tv * tv;
            spriteRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_Step_Value", value);
            spriteRenderer.SetPropertyBlock(materialPropertyBlock);

            // kill if has no HP
            if (HP < 0)
                DestryTile();
        }  
    }


    public void SetIsPlayerOver(bool value)
    {
        IsPlayerOver = value;

        if (IsPlayerOver == true && IsVirtual)
            DestryTile();
    }

    public void DestryTile()
    {
        //Debug.Log("TODO: tile hiding enamiation");
        IsDestroyed = true;
        enabled = false;
        gameObject.SetActive(false);
    }

    public void Heal() => HP = defaultHP;

    private void OnDestroy()
    {
        if (virtualShake != null)
            virtualShake.Kill();
    }
}
