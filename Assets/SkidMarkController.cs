using System;
using System.Collections.Generic;
using System.Text;
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
    }


    private void ShowSkidMarksOnLaneChange()
    {
        var isTurningRight = playerMovement.CurrentVelocity.x > 0 && _prevVelocityX <= 0;
        var isTurningLeft = playerMovement.CurrentVelocity.x < 0 && _prevVelocityX >= 0;
        var isTurning = isTurningRight || isTurningLeft;

        if (!playerMovement.IsMovingBackward && isTurning && Math.Abs(playerMovement.MovementInput.x) > 0.9f)
        {
            if (isTurningRight)
                DrawSkidMark(leftRearTireBottom).Forget();
            if (isTurningLeft)
                DrawSkidMark(rightRearTireBottom).Forget();
        }

        _prevVelocityX = playerMovement.CurrentVelocity.x;
    }

    private async UniTaskVoid DrawSkidMark(Transform tire, float duration = 0.1f)
    {
        var lineRenderer = Instantiate(skidMarkLieLineRenderer, roadSpawner, true);
        var points = new List<Vector3>();

        var elapsedTime = 0f;
        var zed = tire.position.z;
        while (elapsedTime < duration)
        {
            zed += 20f * Time.deltaTime;
            Vector3 newPoint = new Vector3(tire.position.x, tire.position.y, zed);
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