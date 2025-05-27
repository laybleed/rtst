using System.IO;
using UnityEngine;

public class PerformanceLogger : MonoBehaviour
{
    private string filePath;
    private float logInterval = 0.1f; 
    private float timer = 0f;

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "deltaTimeLog.txt");
        File.WriteAllText(filePath, "Frame\tDeltaTime\n"); // заголовки
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= logInterval)
        {
            string line = $"{Time.frameCount}\t{Time.deltaTime.ToString("F6")}";
            File.AppendAllText(filePath, line + "\n");
            timer = 0f;
        }

       // Debug.Log(filePath);
    }
}
