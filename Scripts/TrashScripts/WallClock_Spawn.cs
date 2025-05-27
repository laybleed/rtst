using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallClock_Spawn : MonoBehaviour
{


    public GameObject soundPrefab;

    [SerializeField] private SoundManager soundManager;
    [SerializeField] private AudioClip clip;
    [SerializeField] private float volume;

    void Start()
    {
        Vector3 position = transform.position;

        CreateSoundSource(position);
    }


    private void CreateSoundSource(Vector3 position)
    {
        GameObject soundObject = Instantiate(soundPrefab, position, Quaternion.identity);
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.spatialBlend = 1f;


        //audioSource.rolloffMode |= AudioRolloffMode.Linear;
        soundObject.GetComponentInChildren<AudioSource_ConnectionCheck>().SetMuffle(true);
        





        // Игнорируем delayParent, достаточно одного delay
        audioSource.Play();
        Destroy(soundObject, clip.length);
    }
}
