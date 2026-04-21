using System;
using UnityEditor.Rendering;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] CameraParameters parameters;

    public Transform MainCamera;
    public Transform Origin;

    private float xrot;
    private float yrot;

    public bool FreeCamera = false;

    private float controlScale = 1f;

    // Update is called once per frame
    void Update()
    {
        UpdateCamera();
    }

    public void SetControlScale(float scale)
    {
        controlScale = Mathf.Clamp01(scale);
    }

    void UpdateCamera()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            //FreeCamera = !FreeCamera;
            Rotation();
        }
        // if (!FreeCamera) { return; }

    }



    void Rotation()
    {
        var ox = Input.GetAxis("Mouse X") * parameters.sensitivity * 10f * Time.deltaTime;
        var oy = Input.GetAxis("Mouse Y") * parameters.sensitivity * 10f * Time.deltaTime;
        var ow = Input.GetAxis("Mouse ScrollWheel") * parameters.scrollsensitivity * 100 * Time.deltaTime;
        yrot -= ox;
        xrot -= oy;


        transform.rotation = Quaternion.Euler(xrot, yrot, 0);
        MainCamera.Translate(Vector3.forward * parameters.scrollsensitivity * ow);
    }

}
