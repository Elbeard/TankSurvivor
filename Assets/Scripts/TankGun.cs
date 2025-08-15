using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class TankGun : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private SimpleAudioEvent audioEvent;

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

        // �������� �� ������� �����
        if (fireAction != null)
        {
            fireAction.action.performed += OnFire;
        }
    }

    private void Start()
    {
        //projectilePool.InitializePool();
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
        PlayAudioEffect();
        Projectile projectile = projectilePool.GetProjectile();

        if (projectile != null)
        {
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation;
            projectile.gameObject.SetActive(true);


            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = firePoint.up * projectileSpeed;
            }

            // ������������� ������ �� ��� � �������
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.SetPoolReference(projectilePool);
            }

            projectile.transform.SetParent(null);
        }
    }

    private void PlayAudioEffect()
    {
        AudioClip clip = audioEvent.clips[Random.Range(0, audioEvent.clips.Length)];
        float volume = Random.Range(audioEvent.volume.x, audioEvent.volume.y);
        float pitch = Random.Range(audioEvent.pitch.x, audioEvent.pitch.y);

        AudioPool.Instance.TryPlaySound(clip, transform.position, SoundPriority.High, volume, pitch);
    }
}

