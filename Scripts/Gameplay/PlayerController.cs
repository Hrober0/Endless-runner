using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;
using Audio;

public class PlayerController : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField, Min(1)] private int defaultJumpLength = 1;
    [SerializeField, Min(0)] private float jumpTime = 0.3f;
    [SerializeField, Min(0)] private float jumpTargetableAfterJumpTime = 0.1f;
    [SerializeField, Min(0)] private float jumpDelayTime = 0.2f;
    [SerializeField] private Ease jumpEase = Ease.Linear;
    [SerializeField] private SFXSO jumpSFX;

    [Header("Dif")]
    [SerializeField, Min(0)] private float size = 1;
    [SerializeField] private SFXSO dieSFX;

    private CameraController cameraController;
    private Animator animator;

    [field: SerializeField, ReadOnly] public bool CanKill { get; private set; } = true;

    public bool InJump => jumping != null;
    private IEnumerator jumping = null;
    [field: SerializeField, ReadOnly] public int JumpLength { get; private set; }
    public Vector2Int LastJumpDirection { get; private set; } = Vector2Int.up;

    private readonly List<Vector2Int> pressedDirs = new();

    [SerializeField, ReadOnly] private MapTile currentTile;


    private void Awake()
    {
        cameraController = FindObjectOfType<CameraController>();
        animator = GetComponent<Animator>();

        ResetJumpLength();

        GameMaster.OnPlayModeChanged += OnPlayModeChanged;
    }

    private void OnDestroy()
    {
        GameMaster.OnPlayModeChanged -= OnPlayModeChanged;
        transform.DOKill();
    }

    private void Update()
    {
        if (GameMaster.CurrPlayMode != GameMaster.PlayMode.Play)
            return;

        InputMove();

        CheckDie();
    }

    private void OnPlayModeChanged(GameMaster.PlayMode mode)
    {
        if (mode == GameMaster.PlayMode.Play)
        {
            enabled = true;
            AlignToGrid();

            SetCurrentTile(StageManager.Instance.GetTile(transform.position));
        }
        else
        {
            enabled = false;
        }
    }

    private void InputMove()
    {
        if (InJump)
            return;

        Vector2Int dir = Vector2Int.zero;

        CheckInput(Vector2Int.left);
        CheckInput(Vector2Int.right);
        CheckInput(Vector2Int.down);
        CheckInput(Vector2Int.up);

        if (dir == Vector2Int.zero && pressedDirs.Count > 0)
            dir = pressedDirs[^1];

        if (dir == Vector2Int.zero)
            return;

        LastJumpDirection = dir;
        TryJump(dir * JumpLength, jumpTime, true);


        void CheckInput(Vector2Int checkedDir)
        {
            int x = (int)Input.GetAxisRaw("Horizontal");
            int y = (int)Input.GetAxisRaw("Vertical");

            if ((checkedDir.x == 0 || x == checkedDir.x) && (checkedDir.y == 0 || y == checkedDir.y))
            {
                if (!pressedDirs.Contains(checkedDir))
                {
                    pressedDirs.Add(checkedDir);
                    dir = checkedDir;
                }
            }
            else
                pressedDirs.Remove(checkedDir);
        }
    }


    public void TryJump(Vector2Int jumpOffset, float jumpTimePerTile, bool playSound)
    {
        if (InJump)
        {
            Debug.LogWarning("Can't jump whill jumping");
            return;
        }

        // check jump over top
        if (jumpOffset.y > 0)
        {
            float topPos = cameraController.CameraTopPosition() - size;
            if (transform.position.y > topPos)
                jumpOffset.y = 0;
        }

        // check if has dir
        if (jumpOffset == Vector2Int.zero)
            return;

        jumpOffset = FixJumpOffset(jumpOffset);

        if (jumpOffset.y > jumpOffset.x)
            Invoke(nameof(MakeUnkilable), jumpTargetableAfterJumpTime);

        jumping = Jump(jumpOffset, jumpTimePerTile, playSound);
        StartCoroutine(jumping);
        animator.SetBool("InJump", true);
    }
    public void MakeDash(Vector2Int jumpOffset, float jumpTimePerTile)
    {
        jumpOffset = FixJumpOffset(jumpOffset);

        if (jumpOffset == Vector2Int.zero)
            return;

        if (InJump)
            StopCoroutine(jumping);

        MakeUnkilable();

        jumping = Jump(jumpOffset, jumpTimePerTile, false);
        StartCoroutine(jumping);
        animator.SetBool("InJump", true);
    }
    private IEnumerator Jump(Vector2Int dir, float jumpTimePerTile, bool playSound)
    {
        StageManager stageManager = StageManager.Instance;
        Vector2Int targtGridPos = stageManager.GridPos(transform.position) + dir;
        Vector2 targetPos = stageManager.WorldPos(targtGridPos.x, targtGridPos.y);

        // jump
        if (playSound && jumpSFX != null)
            AudioManager.PlaySFX(jumpSFX);
        SetCurrentTile(null);
        float fixedJumpTime = jumpTimePerTile * dir.magnitude;
        transform.DOLocalMove(targetPos, fixedJumpTime).SetEase(jumpEase);
        yield return new WaitForSeconds(fixedJumpTime);

        // end jump
        AlignToGrid();
        MapTile currentTile = stageManager.GetTile(transform.position);
        SetCurrentTile(currentTile);

        // wait
        yield return new WaitForSeconds(jumpDelayTime);
        jumping = null;
        CanKill = true;
        animator.SetBool("InJump", false);
        CheckDie();
    }
    private Vector2Int FixJumpOffset(Vector2Int jumpOffset)
    {
        StageManager stageManager = StageManager.Instance;
        Vector2Int playerGridPos = stageManager.GridPos(transform.position);
        MapTile targetTile = GetTargetTile();

        // check target tile
        if (targetTile != null)
        {
            // try make jump shorter
            while (CanJumpOn(targetTile) == false)
            {
                if (Mathf.Abs(jumpOffset.x) > Mathf.Abs(jumpOffset.y))
                    jumpOffset.x -= jumpOffset.x > 0 ? 1 : -1;
                else
                    jumpOffset.y -= jumpOffset.y > 0 ? 1 : -1;

                if (jumpOffset == Vector2Int.zero)
                    return Vector2Int.zero;

                targetTile = GetTargetTile();
            }
        }

        return jumpOffset;

        MapTile GetTargetTile() => stageManager.GetTile(stageManager.WorldPos(playerGridPos + jumpOffset));
    }


    public void SetJumpLength(int length) => JumpLength = length;
    public void ResetJumpLength() => JumpLength = defaultJumpLength;


    public void IncreeaseJumpSpeedByPercent(int percent)
    {
        float multi = (100 - percent) / 100f;
        jumpTime *= multi;
        jumpDelayTime *= multi;
    }


    private void CheckDie()
    {
        if (InJump)
            return;

        float bottomPos = cameraController.CameraBottomPosition() - size;
        if (transform.position.y < bottomPos)
        {
            Die();
            return;
        }

        if (currentTile == null || currentTile.IsDestroyed)
        {
            Die();
            return;
        }
    }
    private void Die()
    {
        if (dieSFX != null)
            AudioManager.PlaySFX(dieSFX);

        transform.DOScale(0, 0.6f);
        transform.DOMoveY(transform.position.y - size, 0.6f);

        GameMaster.SetPlayMode(GameMaster.PlayMode.GameOver);
    }
    public void Kill() => Die();
    private void MakeUnkilable() => CanKill = false;

    private void AlignToGrid() => transform.position = StageManager.Instance.AlginPos(transform.position);

    private bool CanJumpOn(MapTile tile)
    {
        if (tile == null)
            return true;

        return tile.Height == 0;
    }

    private void SetCurrentTile(MapTile tile)
    {
        if (currentTile == tile)
            return;

        if (currentTile)
            currentTile.SetIsPlayerOver(false);

        currentTile = tile;

        if (currentTile)
            currentTile.SetIsPlayerOver(true);
    }
}
