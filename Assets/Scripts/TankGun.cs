using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class TankGun : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private Transform firePoint;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference fireAction;

    private float nextFireTime;
    private ProjectilePool projectilePool;

    private void Awake()
    {
        projectilePool = GetComponent<ProjectilePool>();
        if (projectilePool == null)
        {
            projectilePool = gameObject.AddComponent<ProjectilePool>();
        }

        // Подписка на событие ввода
        if (fireAction != null)
        {
            fireAction.action.performed += OnFire;
        }
    }

    private void Start()
    {
        projectilePool.InitializePool();
    }

    private void OnEnable()
    {
        if (fireAction != null)
        {
            fireAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (fireAction != null)
        {
            fireAction.action.Disable();
        }
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        GameObject projectile = projectilePool.GetProjectile();

        if (projectile != null)
        {
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation;
            projectile.SetActive(true);

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = firePoint.up * projectileSpeed;
            }

            // Устанавливаем ссылку на пул в снаряде
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.SetPoolReference(projectilePool);
            }

            projectile.transform.SetParent(null);
        }
    }
}

