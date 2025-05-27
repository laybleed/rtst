using UnityEngine;
using UnityEngine.InputSystem;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager; // Теперь задаём вручную
    [SerializeField] private AudioClip[] stepSounds; // Звуки шагов
    [SerializeField] private float stepInterval = 0.5f; // Время между шагами
    private float stepTimer;

    private void Update()
    {
        bool isMoving = IsPlayerMoving();
        if (!isMoving) return; // Если не двигается, выходим

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

    public void PlayStepSound()  // ПОТОМ ЗАМЕНИТЬ НА PRIVATE
    {
        if (soundManager == null)
        {
            Debug.LogWarning("[SoundEmitter] SoundManager не задан!");
            return;
        }

        if (stepSounds.Length == 0)
        {
            Debug.LogWarning("[SoundEmitter] Нет доступных звуков шагов.");
            return;
        }

        AudioClip randomStepSound = stepSounds[Random.Range(0, stepSounds.Length)];
        soundManager.ProcessSound(transform.position, randomStepSound, 1f);
    }
}
