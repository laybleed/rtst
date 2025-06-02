using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

public class RTST_BenchmarkSpawner : MonoBehaviour
{
    [Header("Benchmark Settings")]
    [SerializeField] private int initialSourceCount = 5;
    [SerializeField] private int iterations = 5;
    [SerializeField] private int multiplier = 5;
    [SerializeField] private int repeatsPerBatch = 5;
    [SerializeField] private float delayBetweenSteps = 1f;

    [Header("Random Positioning")]
    [SerializeField] private float xRange = 5f;
    [SerializeField] private float yRange = 5f;

    [Header("Sound Settings")]
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private AudioClip soundClip;
    [SerializeField] private float volume = 1f;

    private string logFilePath;

    private void Start()
    {
        logFilePath = Application.dataPath + "/D_Logs/RTST_Benchmark_Log.txt";
        if (!File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, "BatchIndex\tRepeat\tSourceCount\tMilliseconds\n");
        }

        StartCoroutine(RunBenchmark());
    }

    private IEnumerator RunBenchmark()
    {
        int currentSourceCount = initialSourceCount;

        for (int batch = 0; batch < iterations; batch++)
        {
            for (int repeat = 0; repeat < repeatsPerBatch; repeat++)
            {
                List<Vector3> positions = new List<Vector3>();
                for (int i = 0; i < currentSourceCount; i++)
                {
                    float offsetX = Random.Range(-xRange, xRange);
                    float offsetY = Random.Range(-yRange, yRange);
                    Vector3 spawnPos = transform.position + new Vector3(offsetX, offsetY, 0f);
                    positions.Add(spawnPos);
                }

                Stopwatch sw = Stopwatch.StartNew();

                foreach (Vector3 pos in positions)
                {
                    soundManager.ProcessSound(pos, soundClip, volume);
                }

                sw.Stop();
                float ms = sw.ElapsedTicks / (float)Stopwatch.Frequency * 1000f;

                string line = $"{batch + 1}\t{repeat}\t{currentSourceCount}\t{ms.ToString("F5").Replace(",", ".")}\n";
                File.AppendAllText(logFilePath, line, Encoding.UTF8);

                yield return new WaitForSeconds(delayBetweenSteps);
            }

            currentSourceCount += multiplier;
            yield return new WaitForSeconds(delayBetweenSteps);
        }

        UnityEngine.Debug.Log("RTST Benchmark завершён. Все замеры записаны.");
    }
}
