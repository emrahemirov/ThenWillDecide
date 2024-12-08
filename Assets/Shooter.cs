using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class Shooter : MonoBehaviour
{
    private List<Transform> _hitPoints;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float fireRateBetweenShots = 0.2f;
    [SerializeField] private float fireRateBetweenHitPoints;

    private float _nextShootTime;

    private void Start()
    {
        _hitPoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            _hitPoints.Add(child);
        }
    }

    private void FixedUpdate()
    {
        Fire();
    }

    private void Fire()
    {
        if (Time.time < _nextShootTime) return;

        FireFromHitPointsAsync().Forget();

        _nextShootTime = Time.time + fireRateBetweenShots;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private async UniTaskVoid FireFromHitPointsAsync()
    {
        foreach (var hitPoint in _hitPoints)
        {
            var bullet = ObjectPoolManager.Instance.Get(PoolKey.Bullet);
            bullet.transform.position = hitPoint.position;
            bullet.transform.rotation = hitPoint.rotation;
            var bulletRigidbody = bullet.GetComponent<Rigidbody>();

            bulletRigidbody.linearVelocity = hitPoint.forward * bulletSpeed;
            ObjectPoolManager.Instance.ReturnWithDelay(PoolKey.Bullet, bullet, 0.5f).Forget();

            if(fireRateBetweenHitPoints > 0f) await UniTask.Delay(System.TimeSpan.FromSeconds(fireRateBetweenHitPoints));
        }
    }
}