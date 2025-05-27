using UnityEngine;
using System.Collections;

public class GameExitTimer : MonoBehaviour
{
    private void Start()
    {
        StartExitCountdown();
    }

    public void StartExitCountdown()
    {
        StartCoroutine(ExitAfterDelay());
    }

    private IEnumerator ExitAfterDelay()
    {
        yield return new WaitForSeconds(61f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Для редактора
#else
        Application.Quit(); // Для билда
#endif
    }
}
