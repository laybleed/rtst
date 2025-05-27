using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioSource_ConnectionCheck : MonoBehaviour
{
    private Transform player;
    private AudioSource source;
    private SurfaceMaterial material;
    private int updated = 2;
    private float beta = 5e-3f; // Можно кастомизировать или получить из глобального SoundManager


    private bool isMuffled = false;
    private float attenuationThroughWall = 0.0497f;


    [SerializeField] private LayerMask reflectionMask;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("SoundReciever")?.transform;
        source = GetComponent<AudioSource>();
        //material = GetComponent<SurfaceType>()?.surfaceMaterial;

        // Можно сразу стартануть с фильтрами, если они были назначены
    }

    private void Update()
    {
        if (player == null) return;



        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (!Physics.Raycast(transform.position - toPlayer.normalized, toPlayer.normalized, distance, reflectionMask) && isMuffled == true && (updated == 0 || updated == 2))
        {
            SoundUnMuffle(toPlayer, distance);
        }
        else if (Physics.Raycast(transform.position - toPlayer.normalized * 0.5f, toPlayer.normalized, distance, reflectionMask) && isMuffled == false && (updated == 1 || updated == 2))
        {
            SoundMuffle(); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }
    }

    public void SetMuffle(bool m)
    {
        isMuffled = m;
    }

    public void SetAttenuationThroughWall(float attenuationThroughWall_OG)
    {
        attenuationThroughWall = attenuationThroughWall_OG;
        //Debug.Log("attenuationThroughWall = " + attenuationThroughWall);
    }

    private void SoundUnMuffle(Vector3 toPlayer, float distance)
    {
        Debug.DrawRay(transform.position - toPlayer.normalized * 0.5f, toPlayer.normalized * distance, Color.yellow, 1f);

        // Прямая видимость есть — пересчитываем
        //Debug.Log("Old: " + source.volume + "attenuationThroughWall: " + attenuationThroughWall + "newVolume: " + source.volume / attenuationThroughWall * 2);


        float newVolume = source.volume / attenuationThroughWall * 2;
        //source.volume = newVolume;

        StartCoroutine(SmoothVolumeChange(source, newVolume, 0.3f)); 



        //float newMaxDistance = -Mathf.Log(0.01f / newVolume) / beta;
        //source.maxDistance = newMaxDistance;

        //// Удаляем фильтры (звук теперь "чистый")
        //var lowPass = GetComponent<AudioLowPassFilter>();
        //var highPass = GetComponent<AudioHighPassFilter>();

        //if (lowPass != null) Destroy(lowPass);
        //if (highPass != null) Destroy(highPass);

        isMuffled = false;

        updated = 1; // больше не обновляем ///СДЕЛАТЬ ПРОВЕРКУ ЧТОБ ЕСЛИ ПРОПАЛ, СНОВА ВЕРНУТЬ ВСЕ ФИЛЬТРЫ
    }

    private void SoundMuffle()
    {
        float newVolume = source.volume * attenuationThroughWall / 2;
        //source.volume = newVolume;

        StartCoroutine(SmoothVolumeChange(source, newVolume, 0.3f));

        //AudioLowPassFilter lowPass = source.gameObject.AddComponent<AudioLowPassFilter>();
        //AudioHighPassFilter highPass = source.gameObject.AddComponent<AudioHighPassFilter>();

        //lowPass.cutoffFrequency = 1911f;
        //highPass.cutoffFrequency = 500f; // Фиксированное значение для среза низких частот

        isMuffled = true;

        updated = 0;
    }

    public IEnumerator SmoothVolumeChange(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        // Проверим фильтры
        var lowPass = source.GetComponent<AudioLowPassFilter>();
        var highPass = source.GetComponent<AudioHighPassFilter>();

        float startLow = lowPass ? lowPass.cutoffFrequency : 22000f;
        float targetLow = lowPass ? 22000f : 1911f;

        float startHigh = highPass ? highPass.cutoffFrequency : 0f;
        float targetHigh = highPass ? 0f : 500f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            source.volume = Mathf.Lerp(startVolume, targetVolume, t);

            if (lowPass) lowPass.cutoffFrequency = Mathf.Lerp(startLow, targetLow, t);
            if (highPass) highPass.cutoffFrequency = Mathf.Lerp(startHigh, targetHigh, t);

            yield return null;
        }

        source.volume = targetVolume;
        if (lowPass) lowPass.cutoffFrequency = targetLow;
        if (highPass) highPass.cutoffFrequency = targetHigh;
    }


}
