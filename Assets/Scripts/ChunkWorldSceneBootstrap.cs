using ChunkWorld;
using UnityEngine;

/// <summary>
/// Связка игры TankSurvivor с модулем ChunkWorld: назначает танк как цель стриминга.
/// </summary>
public class ChunkWorldSceneBootstrap : MonoBehaviour
{
    [SerializeField] private ChunkWorldStreamer _streamer;

    private void Awake()
    {
        if (_streamer == null)
            _streamer = FindObjectOfType<ChunkWorldStreamer>();

        if (_streamer == null)
            return;

        Player player = FindObjectOfType<Player>();
        if (player != null)
            _streamer.SetFollowTarget(player.transform);
    }
}
