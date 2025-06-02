using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayVisualizer : MonoBehaviour
{
    [Header("Ray Settings")]
    public int parentRayCount = 10;
    public int childRayCount = 5;
    public float maxDistance = 30f;
    public float drawDuration = 1.0f;
    public int maxReflections = 2;
    public LayerMask reflectionMask;

    [Header("Materials")]
    public Material[] materials; // 0 - красный (родитель), 1 - зелёный (дочерний)
    public Vector2 textureScale = new Vector2(0.45f, 1f); // Масштаб текстуры

    [Header("Line Settings")]
    public float lineWidth = 0.1f; // Толщина линии

    [Header("Prefabs")]
    public GameObject linePrefab;

    private List<GameObject> activeLines = new List<GameObject>();
    private List<RayInfo>[] raysByLevel;

    private class RayInfo
    {
        public Vector3 origin;
        public Vector3 direction;
        public int depth;
    }

    private void Start()
    {
        StartCoroutine(VisualizeRaysLoop());
    }

    private IEnumerator VisualizeRaysLoop()
    {
        while (true)
        {
            yield return StartCoroutine(VisualizeRays());
            yield return new WaitForSeconds(0.5f);
            ClearLines();
        }
    }

    private IEnumerator VisualizeRays()
    {
        // Подготовка уровней
        raysByLevel = new List<RayInfo>[maxReflections + 1];
        for (int i = 0; i < raysByLevel.Length; i++)
            raysByLevel[i] = new List<RayInfo>();

        // Генерация родительских лучей
        for (int i = 0; i < parentRayCount; i++)
        {
            float angle = 360f / parentRayCount * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            raysByLevel[0].Add(new RayInfo { origin = transform.position, direction = dir, depth = 0 });
        }

        // Рекурсивное построение всех уровней
        for (int depth = 0; depth < maxReflections; depth++)
        {
            foreach (var ray in raysByLevel[depth])
            {
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, maxDistance, reflectionMask))
                {
                    Vector3 incoming = ray.direction;
                    Vector3 normal = hit.normal;
                    Vector3 reflectDir = Vector3.Reflect(incoming.normalized, normal);
                    Vector3 tangent = Vector3.Cross(normal, Vector3.up);

                    for (int j = 0; j < childRayCount; j++)
                    {
                        float offset = -90f + 180f * j / Mathf.Max(1, childRayCount - 1); // равномерно по 180°
                        Vector3 spreadDir = Quaternion.AngleAxis(offset, tangent) * reflectDir;

                        raysByLevel[depth + 1].Add(new RayInfo
                        {
                            origin = hit.point,
                            direction = spreadDir,
                            depth = depth + 1
                        });
                    }
                }
            }
        }

        // Рисуем уровень за уровнем
        for (int depth = 0; depth < raysByLevel.Length; depth++)
        {
            foreach (var ray in raysByLevel[depth])
            {
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, maxDistance, reflectionMask))
                {
                    GameObject lineObj = Instantiate(linePrefab);
                    LineRenderer lr = lineObj.GetComponent<LineRenderer>();

                    int matIndex = (ray.depth == 0) ? 0 : 1;
                    lr.material = materials[matIndex];

                    // Настраиваем масштаб текстуры
                    lr.material.mainTextureScale = textureScale;

                    // Устанавливаем толщину линии
                    lr.startWidth = lineWidth;
                    lr.endWidth = lineWidth;

                    lr.positionCount = 2;
                    lr.SetPosition(0, ray.origin);
                    lr.SetPosition(1, ray.origin); // начальная позиция

                    activeLines.Add(lineObj);

                    StartCoroutine(AnimateLine(lr, ray.origin, hit.point, drawDuration));
                }
            }

            yield return new WaitForSeconds(drawDuration); // Ждём окончания текущего уровня
        }
    }

    private IEnumerator AnimateLine(LineRenderer lr, Vector3 start, Vector3 end, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            Vector3 current = Vector3.Lerp(start, end, t);
            lr.SetPosition(1, current);
            time += Time.deltaTime;
            yield return null;
        }

        lr.SetPosition(1, end);
    }

    private void ClearLines()
    {
        foreach (var go in activeLines)
        {
            Destroy(go);
        }
        activeLines.Clear();
    }
}
