using UnityEngine;

public class NoteAnimation : MonoBehaviour
{
    private float speed;
    private float destroyY;

    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;

        if (transform.position.y <= destroyY)
            Destroy(gameObject);
    }
    
    public void Initialize(float speed, float destroyY)
    {
        this.speed = speed;
        this.destroyY = destroyY;
    }
}