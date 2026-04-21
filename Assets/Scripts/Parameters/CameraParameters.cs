using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu (fileName= "Camera_Parameters", menuName = "ScriptableObjects/Camera") ]
public class CameraParameters : ScriptableObject 
{
    public float sensitivity;
    public float scrollsensitivity;
}
