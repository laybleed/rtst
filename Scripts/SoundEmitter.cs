using UnityEngine;
using UnityEngine.InputSystem;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager; // ������ ����� �������
    [SerializeField] private AudioClip[] stepSounds; // ����� �����
    [SerializeField] private float stepInterval = 0.5f; // ����� ����� ������
    private float stepTimer;

    private void Update()
    {
        bool isMoving = IsPlayerMoving();
        if (!isMoving) return; // ���� �� ���������, �������

        stepTimer += Time.deltaTime;
        if (stepTimer >= stepInterval)
        {
           // PlayStepSound();
            stepTimer = 0f;
        }
    }

    private bool IsPlayerMoving()
    {
        var keyboard = Keyboard.current;
        return keyboard != null && (keyboard.wKey.isPressed || keyboard.aKey.isPressed ||
                                    keyboard.sKey.isPressed || keyboard.dKey.isPressed);
    }

    public void PlayStepSound()  // ����� �������� �� PRIVATE
    {
        if (soundManager == null)
        {
            Debug.LogWarning("[SoundEmitter] SoundManager �� �����!");
            return;
        }

        if (stepSounds.Length == 0)
        {
            Debug.LogWarning("[SoundEmitter] ��� ��������� ������ �����.");
            return;
        }

        AudioClip randomStepSound = stepSounds[Random.Range(0, stepSounds.Length)];
        soundManager.ProcessSound(transform.position, randomStepSound, 1f);
    }
}
