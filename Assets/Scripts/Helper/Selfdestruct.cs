using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selfdestruct : MonoBehaviour
{
    public float Lifetime;

    private void Update()
    {
        Lifetime -= Time.deltaTime;

        if (Lifetime < 0)
            Destroy(gameObject);
    }
}
