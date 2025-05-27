using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    public float openAngle = 90f;         // угол открытия
    public float openDuration = 5f;       // длительность открытия
    public float delayBeforeOpen = 10f;   // задержка перед открытием
    public bool openOnStart = true;

    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float timer = 0f;
    private float delayTimer = 0f;
    private bool isDelaying = false;
    private bool isOpening = false;

    void Start()
    {
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

        if (openOnStart)
        {
            isDelaying = true;
            delayTimer = 0f;
        }
    }

    void Update()
    {
        if (isDelaying)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= delayBeforeOpen)
            {
                isDelaying = false;
                isOpening = true;
                timer = 0f;
            }
        }

        if (isOpening)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / openDuration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
        }
    }

    public void OpenDoor()
    {
        if (!isDelaying && !isOpening)
        {
            startRotation = transform.rotation;
            targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
            isDelaying = true;
            delayTimer = 0f;
        }
    }
}
