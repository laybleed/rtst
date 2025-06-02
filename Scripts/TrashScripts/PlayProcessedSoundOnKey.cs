using cowsins;
using UnityEngine;

public class PlayProcessedSoundOnKey : MonoBehaviour
{
    [Header("Prefab обработанного звука")]
    public GameObject soundPrefab; // Префаб обработанного звука
    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private SoundManager soundManager;

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    PlayStepSound();
        //}
    }

    public void PlayStepSound()  // ПОТОМ ЗАМЕНИТЬ НА PRIVATE
    {
        if (soundManager == null)
        {
            Debug.LogWarning("[SoundEmitter] SoundManager не задан!");
            return;
        }

        if (stepSounds.Length == 0)
        {
            Debug.LogWarning("[SoundEmitter] Нет доступных звуков шагов.");
            return;
        }

        AudioClip randomStepSound = stepSounds[Random.Range(0, stepSounds.Length)];
        soundManager.ProcessSound(transform.position, randomStepSound, 1f);
    }
}
