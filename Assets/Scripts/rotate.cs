using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    Dictionary<KeyCode, bool> keypressed = new Dictionary<KeyCode, bool>();
    void Start()
    {
        keypressed.Add(KeyCode.RightArrow, false);
        keypressed.Add(KeyCode.LeftArrow, false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            keypressed[KeyCode.RightArrow] = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            keypressed[KeyCode.LeftArrow] = true;
        }
        
        if (keypressed[KeyCode.RightArrow])
        {
            Rotate(90.0f);
            snapToLane();
            keypressed[KeyCode.RightArrow] = false;
        }
        if (keypressed[KeyCode.LeftArrow])
        {
            Rotate(-90.0f);
            snapToLane();
            keypressed[KeyCode.LeftArrow] = false;
        }
    }

    void Rotate(float angle)
    {
        Quaternion rotation = transform.rotation;
        rotation *= Quaternion.Euler(0, angle, 0);
        transform.rotation = rotation;

        // Snap the rotation to the nearest 90 degrees to avoid floating point errors
        Vector3 euler = transform.rotation.eulerAngles;
        euler.y = Mathf.Round(euler.y / 90.0f) * 90.0f;
        transform.eulerAngles = euler;
    }


    void snapToLane()
    {

    }

}
