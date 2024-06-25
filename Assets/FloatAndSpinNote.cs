using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatAndSpinNote : MonoBehaviour
{
    Vector3 originalPosition;
    public float floatStrength = 1f; // Strength of the floating effect
    public float spinSpeed = 5f;     // Speed of the spinning effect

    void Start()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = originalPosition + new Vector3(0, Mathf.Sin(Time.time) * floatStrength, 0);
        transform.Rotate(spinSpeed * Time.deltaTime, 0, 0);
        transform.Rotate(spinSpeed * Time.deltaTime, 0, 0);
    }
}
