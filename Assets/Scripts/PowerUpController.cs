using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    public float expiryTime = 2;

    float expiryTimer = 0;

    void Start()
    {
        expiryTimer = 0;
    }

    void Update()
    {
        expiryTimer += Time.deltaTime;

        if(expiryTimer > expiryTime)
        {
            Destroy(gameObject);
        }
    }
}
