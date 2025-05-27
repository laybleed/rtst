using UnityEngine;

public class SpawnOnce : MonoBehaviour
{
    public float speed = 5f; 
    public float interval = 2f; 
    public float distance = 10f; 

    private float timeElapsed = 0f; 

    private void Start()
    {
        call();
    }

    void call()
    {
        GetComponent<SoundEmitter>().PlayStepSound();
    }
}
