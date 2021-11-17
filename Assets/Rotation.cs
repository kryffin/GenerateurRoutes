using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{

    [Range(0f, 50f)]
    public float Speed;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.down * Speed * Time.deltaTime);
    }
}
