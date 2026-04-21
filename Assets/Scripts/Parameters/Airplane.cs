using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class Airplane : MonoBehaviour
{
    [Header("Control Surfaces")]
    public ControlSurface[] elevators;
    public ControlSurface[] aileronsLeft;
    public ControlSurface[] aileronsRight;
    public ControlSurface[] rudders;

    public Rigidbody Rigidbody { get; private set; }
    private Transform _transform;

    private float _throttle = 1.0f;
    private bool _yawDefined = false;


    private readonly Rect _rectSpeed = new Rect(10, 40, 300, 20);
    private readonly Rect _rectThrottle = new Rect(10, 60, 300, 20);
    private readonly Rect _rectGLoad = new Rect(10, 80, 300, 20);

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        _transform = transform;
    }

    private void Start()
    {
        try
        {
            Input.GetAxis("Yaw");
            _yawDefined = true;
        }
        catch (ArgumentException e)
        {
            Debug.LogWarning($"{name}: \"Yaw\" axis not defined in Input Manager. Rudder will not work correctly!\n{e.Message}");
        }
    }

    private void Update()
    {
        float pitchInput = -Input.GetAxis("Vertical");
        float rollInput = Input.GetAxis("Horizontal");
        float yawInput = _yawDefined ? Input.GetAxis("Yaw") : 0f;

        ApplyInputToSurfaces(elevators, pitchInput);
        ApplyInputToSurfaces(aileronsLeft, -rollInput);
        ApplyInputToSurfaces(aileronsRight, rollInput);
        ApplyInputToSurfaces(rudders, yawInput);
    }

    private void ApplyInputToSurfaces(ControlSurface[] surfaces, float input)
    {
        if (surfaces == null) return;
        for (int i = 0; i < surfaces.Length; i++)
        {
            if (surfaces[i] != null)
                surfaces[i].targetDeflection = input;
        }
    }

    private float CalculatePitchG()
    {
        Vector3 localVelocity = _transform.InverseTransformDirection(Rigidbody.velocity);
        Vector3 localAngularVel = _transform.InverseTransformDirection(Rigidbody.angularVelocity);

        float radius = Mathf.Approximately(localAngularVel.x, 0.0f) ? float.MaxValue : localVelocity.z / localAngularVel.x;
        float verticalForce = Mathf.Approximately(radius, 0.0f) ? 0.0f : (localVelocity.z * localVelocity.z) / radius;

        float verticalG = verticalForce / -9.81f;
        verticalG += _transform.up.y * (Physics.gravity.y / -9.81f);

        return verticalG;
    }

    private void OnGUI()
    {
        const float msToKnots = 1.94384f;

       
        GUI.Label(_rectSpeed, $"Speed: {Rigidbody.velocity.magnitude * msToKnots:0.0} knots");
        GUI.Label(_rectThrottle, $"Throttle: {_throttle * 100.0f:0.0}%");
        GUI.Label(_rectGLoad, $"G Load: {CalculatePitchG():0.0} G");
    }
}