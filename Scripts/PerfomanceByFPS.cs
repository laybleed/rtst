using System.IO;
using UnityEngine;

public class PerformanceByFPS : MonoBehaviour
{
    private string filePath;
    private float logInterval = 0.1f; // логировать раз в 0.1 секунды
    private float timer = 0f;
    private int frameCounter = 0;

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "deltaLogFPS.txt");
        File.WriteAllText(filePath, "Time\tFrameCount\n"); // заголовки
    }

    void Update()
    {
        timer += Time.deltaTime;
        frameCounter++;

        if (timer >= logInterval)
        {
            string line = $"{Time.time:F3}\t{frameCounter}";
            File.AppendAllText(filePath, line + "\n");

            timer = 0f;
            frameCounter = 0;
        }
    }
}
