using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

//// Где-то при старте игры (например, в GameManager)
//void InitializeAudioPool()
//{
//    GameObject audioManager = new GameObject("AudioManager");
//    AudioPool audioPool = audioManager.AddComponent<AudioPool>();

//    // Настройки (можно вынести в ScriptableObject)
//    audioPool._audioSourcePrefab = Resources.Load<AudioSource>("AudioSourcePrefab");
//    audioPool._defaultCapacity = 10;
//    audioPool._maxSize = 20;

//    DontDestroyOnLoad(audioManager); // Для переноса между сценами
//}