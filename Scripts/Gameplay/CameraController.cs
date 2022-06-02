using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using System;
using NaughtyAttributes;

[RequireComponent(typeof(PixelPerfectCamera))]
public class CameraController : MonoBehaviour
{
    [Header("Following")]
    [SerializeField] private Transform followinObject;
    [SerializeField, Range(0.1f, 0.9f)] private float horizontalPercentToMove = 0.8f;
    [SerializeField, Range(0.1f, 0.9f)] private float topPercentToMove = 0.8f;

    [Header("Move")]
    [SerializeField] [Range(0f, 0.99f)] private float moveXSmoothness = 0.5f;
    [SerializeField] [Range(0f, 0.99f)] private float moveYSmoothness = 0.5f;
    private Vector2 targetPosition;

    [Header("Zoom")]
    [SerializeField] private int minReferenceY = 540;
    [SerializeField] private bool limitMaxReferenceY = true;
    private float inputResY;

    private PixelPerfectCamera perfectCamera;

    private void Start()
    {
        perfectCamera = GetComponent<PixelPerfectCamera>();

        SetCameraSize(perfectCamera.refResolutionY);
        SetTargetPosition(transform.position);
    }

    private void Update()
    {
        if (followinObject != null)
            Follow();

        UpdateCameraPosition();
    }


    /// <summary>
    /// Count the screen size in world units
    /// </summary>
    /// <returns>Camera size in world units</returns>
    public Vector2 GetCameraWorldSize()
    {   
        if (perfectCamera == null)
            perfectCamera = GetComponent<PixelPerfectCamera>();

        float cameraHeight = perfectCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * Screen.width / Screen.height;
        return new Vector2(cameraWidth, cameraHeight);
    }
    public float CameraTopPosition() => transform.position.y + GetCameraWorldSize().y / 2f;
    public float CameraBottomPosition() => transform.position.y - GetCameraWorldSize().y / 2f;



    /// <summary>
    /// Set camera position
    /// </summary>
    public void SetTargetPosition(Vector2 position) => SetTargetPosition(position.x, position.y);
    public void SetTargetPosition(float posX, float posY) => targetPosition = new(posX, posY);
    public void MoveCamera(Vector2 move) => SetTargetPosition(targetPosition + move);

    private void UpdateCameraPosition()
    {
        transform.position = new(
            Mathf.Lerp(transform.position.x, targetPosition.x, 1 - moveXSmoothness),
            Mathf.Lerp(transform.position.y, targetPosition.y, 1 - moveYSmoothness),
            -10);
    }


    private void Follow()
    {
        Vector2 extance = GetCameraWorldSize() / 2f;

        // horizontal
        float deletaX = followinObject.position.x - transform.position.x;
        float targetMaxDistX = extance.x * horizontalPercentToMove;
        if (Math.Abs(deletaX) > targetMaxDistX)
        {
            float targetX = followinObject.position.x - targetMaxDistX * (deletaX < 0 ? -1 : 1);
            SetTargetPosition(targetX, targetPosition.y);
        }

        // top
        float deletaY = followinObject.position.y - transform.position.y;
        float targetMaxDistY = extance.y * topPercentToMove;
        if (deletaY > targetMaxDistY)
        {
            float targetY = followinObject.position.y - targetMaxDistY + 1;
            SetTargetPosition(targetPosition.x, targetY);
        }
    }

    public void SetCameraSize(int resY)
    {
        int maxRef = limitMaxReferenceY ? Screen.height : 100000;
        inputResY = Mathf.Clamp(resY, minReferenceY, maxRef);
        int resolutionY = (int)inputResY;
        if (resolutionY % 2 != 0)
            resolutionY++;

        Vector2Int sreenResolution = GetResolution();
        
        int resolutionX = -1;
        for (int i = 0; i < 100; i += 2)
        {
            if (Check(resolutionY - i) || Check(resolutionY + i))
                break;

            bool Check(int resY)
            {
                if (resY > maxRef || resY < minReferenceY)
                    return false;

                float resX = resY * sreenResolution.x / (float)sreenResolution.y;
                //Debug.Log("Checked " + resY + " => " + resX);
                if (resX == Mathf.Round(resX))
                {
                    resolutionY = resY;
                    resolutionX = Mathf.RoundToInt(resX);
                    return true;
                }
                return false;
            }
        }

        if (resolutionX == -1)
            Debug.LogWarning($"Dont found good resolution x for y= {resolutionY}");
        else
        {
            perfectCamera.refResolutionY = resolutionY;
            perfectCamera.refResolutionX = resolutionX;
        }
    }

    private Vector2Int GetResolution()
    {
        int width = Screen.width;
        int height = Screen.height;

        int resW, resH;
        for (resH = 2; resH < 30; resH++)
        {
            for (resW = 2; resW < 30; resW++)
            {
                if (width / resW == height / resH)
                    return new Vector2Int(resW, resH);
            }
        }

        Debug.LogError($"Dont found resolution of screean {Screen.width} {Screen.height}");
        return new Vector2Int(16, 9);
    }
}
