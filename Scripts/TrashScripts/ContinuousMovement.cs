using UnityEngine;

public class ContinuousMovement : MonoBehaviour
{
    public float speed = 5f; 
    public float interval = 2f; 
    public float distance = 10f; 

    private float timeElapsed = 0f; 

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        timeElapsed += Time.deltaTime;

        if (timeElapsed >= interval)
        {
            timeElapsed = 0f; 
            call(); 
        }

        if (Mathf.Abs(transform.position.x) >= distance)
        {
            speed = 0f;
        }
    }

    void call()
    {
        GetComponent<SoundEmitter>().PlayStepSound();
    }
}
