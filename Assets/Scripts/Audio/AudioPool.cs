using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AudioPool : SingletonBehaviour<AudioPool>
{
    // Настройки
    [Header("Settings")]
    [SerializeField] private AudioSource _audioSourcePrefab;
    [SerializeField] private int _defaultCapacity = 5;
    [SerializeField] private int _maxSize = 15;

    // Пул и активные источники
    private ObjectPool<AudioSource> _pool;
    private readonly Dictionary<AudioSource, SoundPriority> _activeSources = new();

    private void Awake()
    {
        // Инициализация пула
        _pool = new ObjectPool<AudioSource>(
            createFunc: () => CreateAudioSource(),
            actionOnGet: source => OnSourceGet(source),
            actionOnRelease: source => OnSourceRelease(source),
            actionOnDestroy: source => Destroy(source.gameObject),
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );

        // Предзагрузка источников
        PrewarmPool();

        DontDestroyOnLoad(gameObject);
    }

    private AudioSource CreateAudioSource()
    {
        AudioSource source = Instantiate(_audioSourcePrefab, transform);
        source.gameObject.SetActive(false);
        return source;
    }

    private void OnSourceGet(AudioSource source)
    {
        source.gameObject.SetActive(true);
    }

    private void OnSourceRelease(AudioSource source)
    {
        source.gameObject.SetActive(false);
        source.Stop();
        source.clip = null;
    }

    private void PrewarmPool()
    {
        List<AudioSource> instances = new List<AudioSource>(_defaultCapacity);
        for (int i = 0; i < _defaultCapacity; i++)
        {
            instances.Add(_pool.Get());
        }
        foreach (var instance in instances)
        {
            _pool.Release(instance);
        }
    }

    /// <summary> Проигрывает звук с автоматическим возвратом в пул </summary>
    public bool TryPlaySound(AudioClip clip, Vector3 position, SoundPriority priority, float volume = 1f, float pitch = 1f)
    {
        if (!TryGetAvailableSource(priority, out AudioSource source))
            return false;

        ConfigureSource(source, clip, position, volume, pitch);
        StartCoroutine(ReleaseAfterPlay(source, clip.length));
        return true;
    }

    private bool TryGetAvailableSource(SoundPriority priority, out AudioSource source)
    {
        // 1. Попытка получить свободный источник
        if (_pool.CountInactive > 0)
        {
            source = _pool.Get();
            _activeSources.Add(source, priority);
            return true;
        }

        // 2. Попытка вытеснить низкоприоритетный звук
        foreach (var activePair in _activeSources)
        {
            if (activePair.Value < priority)
            {
                source = activePair.Key;
                source.Stop();
                _activeSources[source] = priority; // Обновляем приоритет
                return true;
            }
        }

        source = null;
        return false;
    }

    private void ConfigureSource(AudioSource source, AudioClip clip, Vector3 position, float volume, float pitch)
    {
        source.clip = clip;
        source.transform.position = position;
        source.volume = volume;
        source.pitch = pitch;
        //source.spatialBlend = is3D ? 1f : 0f; // 3D или 2D звук
        source.Play();
    }

    private IEnumerator ReleaseAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReleaseSource(source);
    }

    private void ReleaseSource(AudioSource source)
    {
        if (_activeSources.ContainsKey(source))
            _activeSources.Remove(source);

        _pool.Release(source);
    }

    /// <summary> Принудительно остановить звук </summary>
    public void StopSound(AudioSource source)
    {
        if (source != null && source.isPlaying)
            ReleaseSource(source);
    }

    /// <summary> Очистить все звуки (например при паузе) </summary>
    public void StopAllSounds()
    {
        foreach (var source in new List<AudioSource>(_activeSources.Keys))
        {
            StopSound(source);
        }
    }
}

public enum SoundPriority
{
    Low,    // Фоновые звуки (ветер, фоновая музыка)
    Medium, // Важные, но не критические (взрывы)
    High    // Критические (выстрелы, интерфейс)
}
