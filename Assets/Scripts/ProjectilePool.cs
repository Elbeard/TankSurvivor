//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Pool;

//public class ProjectilePool : MonoBehaviour
//{
//    [SerializeField] private GameObject projectilePrefab;
//    [SerializeField] private int initialPoolSize = 10;

//    private Queue<GameObject> projectileQueue;

//    public void InitializePool()
//    {
//        projectileQueue = new Queue<GameObject>();

//        for (int i = 0; i < initialPoolSize; i++)
//        {
//            AddNewProjectileToPool();
//        }
//    }

//    private GameObject AddNewProjectileToPool()
//    {
//        GameObject projectile = Instantiate(projectilePrefab, transform);
//        projectile.SetActive(false);
//        projectileQueue.Enqueue(projectile);
//        return projectile;
//    }

//    public GameObject GetProjectile()
//    {
//        // Ищем первый неактивный снаряд в очереди
//        foreach (GameObject projectile in projectileQueue)
//        {
//            if (!projectile.activeInHierarchy)
//            {
//                return projectile;
//            }
//        }

//        // Если все снаряды активны, создаем новый
//        return AddNewProjectileToPool();
//    }

//    public void ReturnProjectile(GameObject projectile)
//    {
//        projectile.SetActive(false);

//        // Сбрасываем физическое состояние
//        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.velocity = Vector2.zero;
//            rb.angularVelocity = 0f;
//        }
//    }
//}

using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;

    private ObjectPool<Projectile> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<Projectile>(
            createFunc: () => Instantiate(projectilePrefab),
            actionOnGet: (projectile) => projectile.gameObject.SetActive(true),
            actionOnRelease: (projectile) => projectile.gameObject.SetActive(false),
            actionOnDestroy: (projectile) => Destroy(projectile.gameObject),
            collectionCheck: true, 
            defaultCapacity: 10,
            maxSize: 20);
    }

    public Projectile GetProjectile()
    {
        return _pool.Get();
    }

    public void ReleaseProjectile(Projectile projectile)
    {
        _pool.Release(projectile);
    }

    private void OnDestroy()
    {
        _pool.Dispose(); 
    }
}