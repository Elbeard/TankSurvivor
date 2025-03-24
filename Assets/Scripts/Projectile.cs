using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask damageLayers;

    private ProjectilePool pool;
    private float spawnTime;

    public void SetPoolReference(ProjectilePool projectilePool)
    {
        pool = projectilePool;
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        transform.SetParent(null);
    }

    private void Update()
    {
        if (Time.time - spawnTime >= lifetime)
        {
            ReturnToPool();
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (((1 << collision.gameObject.layer) & damageLayers) != 0)
    //    {
    //        Health targetHealth = collision.GetComponent<Health>();
    //        if (targetHealth != null)
    //        {
    //            targetHealth.TakeDamage(damage);
    //        }

    //        ReturnToPool();
    //    }
    //}

    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}