using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class PooledSoundObject : MonoBehaviour
{
    private AudioSource source;
    private IObjectPool<GameObject> pool;

    private Coroutine returnRoutine;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void Init(IObjectPool<GameObject> assignedPool)
    {
        pool = assignedPool;
    }

    public void Play(AudioClip clip, float volume, float delay)
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        source.clip = clip;
        source.volume = volume;
        source.PlayDelayed(delay);

        returnRoutine = StartCoroutine(ReturnAfterPlayback(clip.length + delay));
    }

    private IEnumerator ReturnAfterPlayback(float duration)
    {
        yield return new WaitForSeconds(duration);
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (gameObject.activeSelf)
            pool.Release(gameObject);
    }
}
