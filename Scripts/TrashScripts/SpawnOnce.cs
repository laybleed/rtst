using UnityEngine;

public class SpawnOnce : MonoBehaviour
{
    public float delay = 2f; // �������� ����� �������

    private void Start()
    {
        Invoke(nameof(Call), delay);
    }

    void Call()
    {
        GetComponent<SoundEmitter>().PlayStepSound();
    }
}

