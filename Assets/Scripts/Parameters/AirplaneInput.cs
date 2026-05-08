using UnityEngine;

[RequireComponent(typeof(Airplane))]
public class AirplaneInput : MonoBehaviour
{
    private Airplane _airplane;
    private bool _yawDefined = false;

    private void Awake()
    {
        _airplane = GetComponent<Airplane>();
    }

    private void Start()
    {
        try { Input.GetAxis("Yaw"); _yawDefined = true; }
        catch (System.ArgumentException) { Debug.LogWarning("Yaw axis not defined in Input Manager."); }
    }

    private void Update()
    {
        float pitchInput = -Input.GetAxis("Vertical");
        float rollInput = Input.GetAxis("Horizontal");
        float yawInput = _yawDefined ? Input.GetAxis("Yaw") : 0f;
        float throttleDelta = 0f;
        if (Input.GetKey(KeyCode.LeftShift)) throttleDelta += Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftControl)) throttleDelta -= Time.deltaTime;
        _airplane.SetControlInputs(pitchInput, rollInput, yawInput, throttleDelta);
    }
}