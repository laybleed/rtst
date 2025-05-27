using System.IO;
using UnityEngine;

public class PrfomanceByTime : MonoBehaviour
{
    private string filePath;
    private float logInterval = 0.1f; // ���������� ��� � 0.1 �������
    private float timer = 0f;

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "deltaTimeByTime.txt");
        File.WriteAllText(filePath, "Time\tDeltaTime\n"); // ���������
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= logInterval)
        {
            string line = $"{Time.time:F3}\t{Time.deltaTime:F6}";
            File.AppendAllText(filePath, line + "\n");
            timer = 0f;
        }
    }
}
