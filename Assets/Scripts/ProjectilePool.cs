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
            actionOnDestroy: (projectile) =>
            {
                // При Stop Play Unity уже мог уничтожить снаряд (unparent в OnEnable).
                if (projectile)
                    Destroy(projectile.gameObject);
            },
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
        if (_pool == null)
            return;

        // При выходе из Play не вызываем Dispose — Unity сам очистит сцену.
        if (!Application.isPlaying)
        {
            _pool = null;
            return;
        }

        _pool.Dispose();
        _pool = null;
    }
}