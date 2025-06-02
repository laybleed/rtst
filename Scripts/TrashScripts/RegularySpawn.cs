using UnityEngine;
using System.Collections;

public class RegularySpawn : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private bool RTST = true;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private float delayBetweenSteps = 0.1f;
    [SerializeField] private float volume = 1f;

    [Header("Timing Settings")]
    [SerializeField] private float startDelay = 0f; // через сколько секунд запускать звук

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource && !RTST)
        {
            Debug.LogWarning("AudioSource не найден на объекте, а RTST выключен. Ѕудет ошибка.");
        }

        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        // ∆дЄм перед запуском
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        StartCoroutine(StepLoop());
    }

    private IEnumerator StepLoop()
    {
        while (true)
        {
            AudioClip clip = stepSounds[Random.Range(0, stepSounds.Length)];

            if (RTST)
            {
                soundManager.ProcessSound(transform.position, clip, volume);
            }
            else
            {
                audioSource.clip = clip;
                audioSource.volume = volume;
                audioSource.Play();
            }

            yield return new WaitForSeconds(clip.length + delayBetweenSteps);
        }
    }
}
