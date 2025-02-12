using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class SkidMarkController : MonoBehaviour
{
    [SerializeField] private LineRenderer skidMarkLieLineRenderer;
    [SerializeField] private Transform leftRearTireBottom;
    [SerializeField] private Transform rightRearTireBottom;

    private Transform roadSpawner;
    private PlayerMovement playerMovement;
    private float _prevVelocityX;
    private float _prevVelocityZ;


    private void Start()
    {
        roadSpawner = GameObject.FindGameObjectWithTag("RoadSpawner").transform;
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        ShowSkidMarksOnLaneChange();
        ShowSkidMarksOnMovingBackwards();
    }


    private void ShowSkidMarksOnLaneChange()
    {
        var isTurningRight = playerMovement.CurrentVelocity.x > 0 && _prevVelocityX <= 0;
        var isTurningLeft = playerMovement.CurrentVelocity.x < 0 && _prevVelocityX >= 0;
        var isTurning = isTurningRight || isTurningLeft;

        if (!playerMovement.IsMovingBackwards && isTurning && Math.Abs(playerMovement.MovementInput.x) > 0.9f)
        {
            if (isTurningRight)
                DrawSkidMark(leftRearTireBottom, 0.2f).Forget();
            if (isTurningLeft)
                DrawSkidMark(rightRearTireBottom, 0.2f).Forget();
        }

        _prevVelocityX = playerMovement.CurrentVelocity.x;
    }

    private void ShowSkidMarksOnMovingBackwards()
    {
        var isTrue = playerMovement.CurrentVelocity.z < -2 && _prevVelocityZ >= -2;

        if (isTrue && playerMovement.MovementInput.z < -0.9f && Math.Abs(playerMovement.MovementInput.x) < 0.6f)
        {
            DrawSkidMarkB(leftRearTireBottom, 0.5f).Forget();
            DrawSkidMarkB(rightRearTireBottom, 0.5f).Forget();
        }

        _prevVelocityZ = playerMovement.CurrentVelocity.z;
    }

    private async UniTaskVoid DrawSkidMark(Transform tire, float duration)
    {
        var lineRenderer = Instantiate(skidMarkLieLineRenderer, roadSpawner, true);
        var points = new List<Vector3>();

        var elapsedTime = 0f;
        var zed = tire.position.z;
        while (elapsedTime < duration)
        {
            zed += 20f * Time.deltaTime;
            var newPoint = new Vector3(tire.position.x, tire.position.y, zed);
            if (points.Count == 0 || Vector3.Distance(points[^1], newPoint) > 0.1f)
            {
                points.Add(newPoint);
                lineRenderer.positionCount = points.Count;
                lineRenderer.SetPositions(points.ToArray());
            }

            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }
    }

    private async UniTaskVoid DrawSkidMarkB(Transform tire, float duration)
    {
        var lineRenderer = Instantiate(skidMarkLieLineRenderer, roadSpawner, true);
        var points = new List<Vector3>();

        var elapsedTime = 0f;
        var zed = tire.position.z + 5;
        while (elapsedTime < duration)
        {
            zed += playerMovement.CurrentVelocity.z * 0.4f * Time.deltaTime;
            var newPoint = new Vector3(tire.position.x, tire.position.y, zed);
            if (points.Count == 0 || Vector3.Distance(points[^1], newPoint) > 0.1f)
            {
                points.Add(newPoint);
                lineRenderer.positionCount = points.Count;
                lineRenderer.SetPositions(points.ToArray());
            }

            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }
    }
}