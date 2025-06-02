using cowsins;
using UnityEngine;

public class PlayProcessedSoundOnKey : MonoBehaviour
{
    [Header("Prefab ������������� �����")]
    public GameObject soundPrefab; // ������ ������������� �����
    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private SoundManager soundManager;

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    PlayStepSound();
        //}
    }

    public void PlayStepSound()  // ����� �������� �� PRIVATE
    {
        if (soundManager == null)
        {
            Debug.LogWarning("[SoundEmitter] SoundManager �� �����!");
            return;
        }

        if (stepSounds.Length == 0)
        {
            Debug.LogWarning("[SoundEmitter] ��� ��������� ������ �����.");
            return;
        }

        AudioClip randomStepSound = stepSounds[Random.Range(0, stepSounds.Length)];
        soundManager.ProcessSound(transform.position, randomStepSound, 1f);
    }
}
