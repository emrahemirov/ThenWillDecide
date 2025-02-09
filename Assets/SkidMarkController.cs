using System;
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
    private Vector3 _prevVelocity;

    private void Start()
    {
        roadSpawner = GameObject.FindGameObjectWithTag("RoadSpawner").transform;
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void FixedUpdate()
    {
        VelocityChange();
    }


    private void VelocityChange()
    {
        var isTurningRight = playerMovement.CurrentVelocity.x > 0 && _prevVelocity.x <= 0;
        var isTurningLeft = playerMovement.CurrentVelocity.x < 0 && _prevVelocity.x >= 0;

        if ((isTurningRight || isTurningLeft) && Math.Abs(playerMovement.MovementInput.x) > 0.9f)
        {
            if (isTurningRight)
                IncreaseSkidMarkLength(leftRearTireBottom, playerMovement.MovementInput.z >= 0 ? 5 : -5);
            if (isTurningLeft)
                IncreaseSkidMarkLength(rightRearTireBottom, playerMovement.MovementInput.z >= 0 ? -5 : 5);
        }

        _prevVelocity = playerMovement.CurrentVelocity;
    }


    private async void IncreaseSkidMarkLength(Transform tire, float rotateY)
    {
        const float targetScaleZ = -1.5f;
        const float duration = 0.1f;
        var lineRenderer = Instantiate(skidMarkLieLineRenderer, tire.position,
            Quaternion.Euler(0, rotateY, 0), tire);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(1, new Vector3(0, 0, 0));

        var timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            var newScaleZ = Mathf.Lerp(0, targetScaleZ, timeElapsed / duration);

            lineRenderer.SetPosition(1, new Vector3(0, 0, newScaleZ));

            timeElapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        lineRenderer.transform.parent = roadSpawner;
    }
}