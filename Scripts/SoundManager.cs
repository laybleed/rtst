using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Pool;
using UnityEngine.Profiling;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.UI.Image;

public class SoundManager : MonoBehaviour
{
    public int maxReflections = 5;
    public float maxDistance = 50f;
    public LayerMask reflectionMask;
    public GameObject soundPrefab;
    public int numRays = 15;
    public float soundReceivableRadius = 1.5f;
    public bool SimulateWaveTrace = false; // Включает симуляцию волнового распространения
    public bool useMultipleReflections = false;
    public int numReflectedRays = 5;
    public bool useReverbProcessing = false;
    [SerializeField] private AttenuationType attenuationMode = AttenuationType.StandardExponential;
    [SerializeField] private float customBeta = 5e-3f;
    public LayerMask playerSoundTriggerMask;
    public bool useFrequencyFiltering = true; // Включение фильтрации частот
    public float blueTraceEchoOffset = 1.0f;

    private List<Vector3> debugSoundPositions = new List<Vector3>(); // Список точек для дебага
    private List<Vector3> debugSpheres = new List<Vector3>();

    private int measurementIndex = 0;
    private string logFilePath = "E:/Unity_Projects/RealTimeSoundTracing_b01/Assets/D_Logs/CastSoundRays_Log.txt";
    private System.Diagnostics.Stopwatch stopwatch = new Stopwatch();


    private ObjectPool<GameObject> soundPool;




    public bool drawSpheres = false;

    static readonly ProfilerMarker soundRayMarker = new ProfilerMarker("SoundManager.CastSoundRay");

    private List<GizmoSphere> spheresToDraw = new List<GizmoSphere>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var sphere in spheresToDraw)
        {
            Gizmos.DrawWireSphere(sphere.position, sphere.radius);
        }
    }

    private struct GizmoSphere
    {
        public Vector3 position;
        public float radius;
        public GizmoSphere(Vector3 pos, float rad)
        {
            position = pos;
            radius = rad;
        }
    }

    //private void Start()
    //{
    //    //logFilePath = Path.Combine(Application.dataPath, "D_Logs", "CastSoundRays_Log.txt");
    //    //stopwatch = new Stopwatch();

    //    //// Если файл уже есть — начни с новой строки
    //    //File.WriteAllText(logFilePath, "Index\tTimeMs\n");
    //}

    private void Awake()
    {
        soundPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject go = Instantiate(soundPrefab);
                go.AddComponent<PooledSoundObject>();
                return go;
            },
            actionOnGet: obj =>
            {
                obj.SetActive(true);
            },
            actionOnRelease: obj =>
            {
                obj.SetActive(false);
            },
            actionOnDestroy: obj =>
            {
                Destroy(obj);
            },
            collectionCheck: false,
            defaultCapacity: 20,
            maxSize: 1000
        );
    }


    private IEnumerator RemoveSphereAfterDelay(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        spheresToDraw.RemoveAll(sphere => sphere.position == position);
    }

    public enum AttenuationType
    {
        StandardExponential,
        MaterialBased,
        CustomBeta
    }

    public void ProcessSound(Vector3 origin, AudioClip clip, float volume)
    {
        //Debug.Log($"[SoundManager] Получен звук: {clip.name} с громкостью {volume}");

        // Определяем материал поверхности в начальной точке
        SurfaceMaterial material = null;
        //Collider hitCollider = Physics.OverlapSphere(origin, 0.1f, reflectionMask).FirstOrDefault();
        //if (hitCollider != null)
        //{
        //    SurfaceType surface = hitCollider.GetComponent<SurfaceType>();
        //    if (surface != null)
        //    {
        //        material = surface.surfaceMaterial;
        //    }
        //}



        // Создаём звук в начальной точке с учетом материала
        CreateSoundSource(origin, clip, volume, 0f, material);

        // Запускаем лучи
        CastSoundRays(origin, clip, volume);

        //GetClipLoudnessInDB(clip);
    }



    private void CastSoundRays(Vector3 origin, AudioClip clip, float volume)
    {
        //Profiler.BeginSample("MyPieceOfCode");
        soundRayMarker.Begin();

        stopwatch.Reset();
        stopwatch.Start();

        for (int i = 0; i < numRays; i++)
        {
            float horizontalAngle = (360f / numRays) * i;
            float verticalAngle = 0f;
            Vector3 direction = Quaternion.Euler(verticalAngle, horizontalAngle, 0) * Vector3.forward;
            CastSoundRay(origin, direction, clip, volume, maxReflections);
        }

        stopwatch.Stop();
        soundRayMarker.End();
        //Profiler.EndSample();

        // Запись в файл
        double elapsedTime = stopwatch.Elapsed.TotalMilliseconds;
        File.AppendAllText(logFilePath, $"{measurementIndex}\t{elapsedTime}\n");
        measurementIndex++;
    }



    private void CastSoundRay(Vector3 origin, Vector3 direction, AudioClip clip, float volume, int reflectionsLeft, float totalDistance = 0)
    {
        if (volume < 0.01f || reflectionsLeft <= 0) return;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, maxDistance, reflectionMask))
        {
            float distance = Vector3.Distance(origin, hit.point);
            float beta = GetBeta(hit.collider);
            float newVolume = volume * Mathf.Exp(-beta * distance);
            float delay = distance / 343f;



            if (drawSpheres)
            {
                spheresToDraw.Add(new GizmoSphere(hit.point, soundReceivableRadius));
                StartCoroutine(RemoveSphereAfterDelay(hit.point, 1.5f));
            }//DrawSpheres || OLD

            UnityEngine.Debug.DrawRay(origin, direction * distance, Color.red, 1f);

            GameObject player = GameObject.FindGameObjectWithTag("SoundReciever");
            if (player != null)
            {
                Vector3 toPlayer = player.transform.position - hit.point;
                float blueDistance = toPlayer.magnitude;

                UnityEngine.Debug.DrawRay(hit.point - toPlayer.normalized * 0.1f, toPlayer, Color.blue, 1f);

                if (!Physics.Raycast(hit.point - toPlayer.normalized * 0.1f, toPlayer.normalized, blueDistance, reflectionMask))
                {
                    SurfaceMaterial material = null;
                    SurfaceType surface = hit.collider.GetComponent<SurfaceType>();
                    if (surface != null)
                    {
                        material = surface.surfaceMaterial;
                    }

                    float maxSoundDistance = -Mathf.Log(0.01f / newVolume) / beta;
                    float totalDelay = (distance + blueDistance) / 343f;
                    float finalVolume = newVolume * Mathf.Exp(-beta * blueDistance);


                    if (blueDistance > totalDistance + distance)
                    {
                        CreateSoundSource(hit.point, clip, finalVolume, totalDelay, material, maxSoundDistance);
                    }
                }
                else
                {
                    //  Столкнулись со стеной — ищем, насколько она толстая
                    Vector3 inward = direction.normalized;
                    RaycastHit secondHit;
                    float thickness = 1f; // дефолт

                    if (hit.collider.Raycast(new Ray(hit.point + inward * 0.2f, inward), out secondHit, 2f))
                    {
                        thickness = Vector3.Distance(hit.point, secondHit.point);
                    }

                    //  Получаем материал
                    SurfaceMaterial material = null;
                    SurfaceType surface = hit.collider.GetComponent<SurfaceType>();
                    if (surface != null)
                        material = surface.surfaceMaterial;

                    float resistance = material != null ? material.penetrationResistance : 10f;

                    //  Затухание через материал
                    float attenuationThroughWall = Mathf.Exp(-resistance * thickness);

                    float muffledVolume = newVolume * attenuationThroughWall;
                    float muffledDelay = (distance + blueDistance) / 343f;

                    float maxSoundDistance = 2f;

                    if (muffledVolume > 0.01f)
                    {
                        CreateSoundSource(hit.point, clip, muffledVolume, muffledDelay, material, maxSoundDistance, true, attenuationThroughWall);
                    }
                }
            }

            if (false) { 
            //Collider[] colliders = Physics.OverlapSphere(hit.point, soundReceivableRadius, playerSoundTriggerMask);
                //foreach (var col in colliders)
                //{
                //    if (col.CompareTag("Player") && col.CompareTag("KUSOKGOVNATUPOYGPT"))
                //    {
                //        Vector3 toPlayer = (col.transform.position - hit.point).normalized;
                //        float dot = Vector3.Dot(toPlayer, direction);

                //        if (!SimulateWaveTrace || dot > 0)
                //        {
                //            SurfaceMaterial material = null;
                //            SurfaceType surface = hit.collider.GetComponent<SurfaceType>();
                //            if (surface != null)
                //            {
                //                material = surface.surfaceMaterial;
                //            }

                //            CreateSoundSource(hit.point, clip, newVolume, delay, material);
                //        }
                //        break;
                //    }
                //}
            } //ДРЕВНЯЯ РЕАЛИЗАЦИЯ ЧЕРЕЗ СФЕРЫ ОВЕРЛАПА




            if (false)
            {
                //for (int i = 0; i < numReflectedRays; i++)
                //{
                //    //Vector3 reflectedDirection = Vector3.Reflect(direction, hit.normal);
                //    //float horizontalAngle = (180f / numReflectedRays) * i;
                //    //float verticalAngle = Random.Range(-45f, 45f);
                //    //Vector3 spreadDirection = Quaternion.Euler(verticalAngle, horizontalAngle, 0) * reflectedDirection;

                //    //Debug.DrawRay(hit.point, spreadDirection * (maxDistance / 2), Color.green, 1f);
                //    //CastSoundRay(hit.point, spreadDirection, clip, newVolume, reflectionsLeft - 1);
                //}
            } //MULTIPLE || OLD

            if (useMultipleReflections)
            {
                for (int i = 0; i < numReflectedRays; i++)
                {
                    Vector3 reflectedDirection = Vector3.Reflect(direction, hit.normal);

                    // Ось вращения — перпендикуляр к отражённому направлению и нормали
                    Vector3 tangent = Vector3.Cross(reflectedDirection, hit.normal).normalized;
                    if (tangent == Vector3.zero) tangent = Vector3.up; // На всякий случай, если вдруг совпадение

                    float angle = (-90f) + (180f / (numReflectedRays - 1)) * i; // равномерно по 180°
                    float verticalAngle = Random.Range(-45f, 45f);
                    //Quaternion rotation = Quaternion.AngleAxis(angle, tangent);
                    Quaternion rotation = Quaternion.Euler(angle, verticalAngle, 0);
                    Vector3 spreadDirection = rotation * reflectedDirection;

                    //Debug.DrawRay(hit.point, spreadDirection * (maxDistance / 2), Color.green, 1f);

                    totalDistance += distance;

                    CastSoundRay(hit.point, spreadDirection, clip, newVolume, reflectionsLeft - 1, totalDistance);

                }
            }
        }
    }

    private float GetBeta(Collider hitCollider)
    {
        switch (attenuationMode)
        {
            case AttenuationType.StandardExponential:
                return 5e-3f;
            case AttenuationType.MaterialBased:
                SurfaceType surface = hitCollider.GetComponent<SurfaceType>();
                return surface ? surface.surfaceMaterial.absorptionCoefficient : 5e-3f;
            case AttenuationType.CustomBeta:
                return customBeta;
            default:
                return 5e-3f;
        }
    }

    //private void CreateSoundSource(Vector3 position, AudioClip clip, float volume, float delay, SurfaceMaterial material, float maxDistanceOverride = -1f, bool isMuffled = false, float attenuationThroughWall = 0)
    //{
    //    GameObject soundObject = Instantiate(soundPrefab, position, Quaternion.identity);
    //    AudioSource audioSource = soundObject.GetComponent<AudioSource>();
    //    audioSource.clip = clip;
    //    audioSource.volume = volume;
    //    audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    //    audioSource.spatialBlend = 1f;

    //    if (isMuffled)
    //    {
    //        //audioSource.rolloffMode |= AudioRolloffMode.Linear;
    //        audioSource.volume = volume * 0.5f;
    //        soundObject.GetComponentInChildren<AudioSource_ConnectionCheck>().SetMuffle(true);
    //        soundObject.GetComponentInChildren<AudioSource_ConnectionCheck>().SetAttenuationThroughWall(attenuationThroughWall);
    //    }

    //    GameObject player = GameObject.FindGameObjectWithTag("SoundReciever");
    //    if (player == null)
    //    {
    //        Destroy(soundObject);
    //        return;
    //    }

    //    Vector3 toPlayer = player.transform.position - position;
    //    float distance = toPlayer.magnitude;
    //    float betaTemp = 5e-3f;

    //    // Если maxDistanceOverride задан — используем его
    //    if (maxDistanceOverride > 0f)
    //    {
    //        audioSource.maxDistance = maxDistanceOverride;
    //    }
    //    else
    //    {
    //        float predictedVolume = volume * Mathf.Exp(-betaTemp * distance);
    //        float predictedMaxDistance = -Mathf.Log(0.01f / predictedVolume) / betaTemp;
    //        audioSource.maxDistance = predictedMaxDistance;
    //    }

    //    // Только для немафленных звуков проверяем на прямую видимость
    //    if (!isMuffled && Physics.Raycast(position, toPlayer.normalized, distance, reflectionMask))
    //    {
    //        Destroy(soundObject);
    //        return;
    //    }

    //    // Применяем фильтрацию, если есть материал
    //    if (material != null && useFrequencyFiltering)
    //    {
    //        ApplyFilters(audioSource, material);
    //    }

    //    // Игнорируем delayParent, достаточно одного delay
    //    audioSource.PlayDelayed(delay);
    //    Destroy(soundObject, clip.length + delay + 0.5f);
    //}

    private void CreateSoundSource(Vector3 position, AudioClip clip, float volume, float delay, SurfaceMaterial material, float maxDistanceOverride = -1f, bool isMuffled = false, float attenuationThroughWall = 0f)
    {
        GameObject soundObject = soundPool.Get();
        soundObject.transform.position = position;

        AudioSource audioSource = soundObject.GetComponent<AudioSource>();
        PooledSoundObject pooled = soundObject.GetComponent<PooledSoundObject>();
        pooled.Init(soundPool); // обязательно перед проигрыванием

        GameObject player = GameObject.FindGameObjectWithTag("SoundReciever");
        if (player == null)
        {
            soundPool.Release(soundObject);
            return;
        }

        Vector3 toPlayer = player.transform.position - position;
        float distance = toPlayer.magnitude;
        float betaTemp = 5e-3f;

        var connectionCheck = soundObject.GetComponentInChildren<AudioSource_ConnectionCheck>();
        connectionCheck.SetUpdated();
        connectionCheck.SetMuffle(false);

        // Проверка прямой видимости (только для немафленных)
        if (!isMuffled && Physics.Raycast(position, toPlayer.normalized, distance, reflectionMask))
        {
            soundPool.Release(soundObject);
            return;
        }

        // Расчёт maxDistance
        float finalMaxDistance;
        if (maxDistanceOverride > 0f)
        {
            finalMaxDistance = maxDistanceOverride;
        }
        else
        {
            float predictedVolume = volume * Mathf.Exp(-betaTemp * distance);
            finalMaxDistance = -Mathf.Log(0.01f / predictedVolume) / betaTemp;
        }



        // Обработка маффлинга
        if (isMuffled)
        {
            volume *= 0.5f;
           // var connectionCheck = soundObject.GetComponentInChildren<AudioSource_ConnectionCheck>();
            if (connectionCheck != null)
            {
                connectionCheck.SetMuffle(true);
                //connectionCheck.SetUpdated();
                connectionCheck.SetAttenuationThroughWall(attenuationThroughWall);
            }
        }

        // Применение фильтрации по материалу
        if (useFrequencyFiltering && material != null)
        {
            AudioLowPassFilter filter = soundObject.GetComponent<AudioLowPassFilter>();
            if (filter == null)
                filter = soundObject.AddComponent<AudioLowPassFilter>();

            filter.enabled = true;
            filter.cutoffFrequency = material.lowPassCutoff;
        }

        // Воспроизведение и автоматический возврат в пул
        pooled.Play(clip, volume, delay);

        // Установка параметров источника (maxDistance, rolloff и spatialBlend)
        audioSource.maxDistance = finalMaxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.spatialBlend = 1f;
    }


    private float GetClipLoudnessInDB(AudioClip clip)
    {
        if (clip == null) return -80f; // Тишина

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += samples[i] * samples[i]; // квадрат амплитуды

        float rms = Mathf.Sqrt(sum / samples.Length); // root mean square
        float dB = 20f * Mathf.Log10(rms + 1e-7f); // смещение, чтоб не упасть в лог(0)

        UnityEngine.Debug.Log(samples.Length);


        return Mathf.Clamp(dB, -80f, 0f); // Ограничение до нормального диапазона
    }


    private void ApplyFilters(AudioSource source, SurfaceMaterial material)
    {
        AudioLowPassFilter lowPass = source.gameObject.AddComponent<AudioLowPassFilter>();
        AudioHighPassFilter highPass = source.gameObject.AddComponent<AudioHighPassFilter>();

        //// m = плотность * толщина (кг/м2)
        //float m = materialDensity * thicknessMeters;

        //// TLtarget — целевое затухание в децибелах (12 дБ)
        //float TLtarget = 12.0f;

        //// fc = частота среза (в Гц)
        //float fc = Mathf.Pow(10f, (TLtarget + 47f) / 20f) / m;

        lowPass.cutoffFrequency = material.lowPassCutoff;
        highPass.cutoffFrequency = 2f; // Фиксированное значение для среза низких частот

        source.pitch *= material.pitchModifier; // Изменяем питч звука
        UnityEngine.Debug.Log("Sound Pitched!!!");
    }


    private IEnumerator RemoveDebugSphere(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        debugSpheres.Remove(position);
    }

}