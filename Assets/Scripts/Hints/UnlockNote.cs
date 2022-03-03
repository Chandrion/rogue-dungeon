using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockNote : MonoBehaviour
{
    public float Speed, Duration;

    private float lifetime = 0;

    private void Update()
    {
        lifetime += Time.deltaTime;

        transform.position += Vector3.up * Speed * Time.deltaTime;

        if (lifetime > Duration)
            Destroy(gameObject);
    }

}
