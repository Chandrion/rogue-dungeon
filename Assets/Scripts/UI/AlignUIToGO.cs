using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignUIToGO : MonoBehaviour
{
    public GameObject AlignTo;

    public Vector3 Offset;

    private void LateUpdate()
    {
        if (!AlignTo)
            Destroy(gameObject);
        else
            transform.position = Camera.main.WorldToScreenPoint(AlignTo.transform.position + Offset);
    }
}
