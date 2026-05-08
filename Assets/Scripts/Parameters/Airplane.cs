using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class Airplane : MonoBehaviour
{
    [Header("Custom Physics Properties")]
    public float mass = 11000f;
    public Vector3 inertiaTensor = new Vector3(100000f, 300000f, 400000f);
    public LayerMask groundMask = ~0;

    [Header("Engines")]
    public bool engineOn = false;
    public Engines engineData;
    public Transform[] engineExhausts;

    [Header("Control Surfaces")]
    public ControlSurface[] elevators;
    public ControlSurface[] aileronsLeft;
    public ControlSurface[] aileronsRight;
    public ControlSurface[] rudders;

    public Vector3 LinearVelocity { get; private set; }
    public Vector3 AngularVelocity { get; private set; }

    [Range(0f, 1f)]
    public float throttle = 0.0f;

    private Vector3 _accumulatedForce;
    private Vector3 _accumulatedTorque;

    private Transform _transform;
    private Rigidbody _rb;

    // GUI Rects
    private readonly Rect _rectSpeed = new Rect(10, 40, 300, 20);
    private readonly Rect _rectThrottle = new Rect(10, 60, 300, 20);
    private readonly Rect _rectGLoad = new Rect(10, 80, 300, 20);

    private void Awake()
    {
        _transform = transform;
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
    }

    // method: Accepts commands from AirplaneInput.cs
    public void SetControlInputs(float pitch, float roll, float yaw, float throttleDelta)
    {
        if (engineOn)
        {
            throttle = Mathf.Clamp01(throttle + throttleDelta);
        }
        else
        {
            throttle = 0f;
        }

        ApplyInputToSurfaces(elevators, pitch);
        ApplyInputToSurfaces(aileronsLeft, -roll);
        ApplyInputToSurfaces(aileronsRight, roll);
        ApplyInputToSurfaces(rudders, yaw);
    }

    public void AddForceAtPosition(Vector3 force, Vector3 position)
    {
        _accumulatedForce += force;
        Vector3 r = position - _transform.position;
        _accumulatedTorque += Vector3.Cross(r, force);
    }

    public Vector3 GetPointVelocity(Vector3 worldPoint)
    {
        Vector3 r = worldPoint - _transform.position;
        return LinearVelocity + Vector3.Cross(AngularVelocity, r);
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // TODO: ADJUST COLLISION RADIUS
        // Change this value (for example, 1.5f or 1.0f) if the plane appears to float above the ground
        // or, conversely, the wheels appear to be sinking into the textures.
        float collisionRadius = 2.0f;

        _accumulatedForce = Physics.gravity * mass;
        _accumulatedTorque = Vector3.zero;

        ApplyEngineThrust();
        HandleGroundPhysics(dt, collisionRadius, out bool isGrounded, out RaycastHit groundHit);
        IntegratePhysics(dt);
        ApplyMovementAndCollisions(dt, collisionRadius, isGrounded, groundHit);
    }

    private void ApplyEngineThrust()
    {
        if (!engineOn || engineData == null || engineExhausts == null || engineExhausts.Length == 0) return;

        // TODO: BALANCE FIX (FLIP ON TAKEOFF PROBLEM)
        // If pressing the throttle causes the airplane to do a "Cobra" and flip over on the spot:
        // Your engines (RD33Left / RD33Right empty objects) are located too LOW relative to the center of mass (Airplane's Pivot).
        // Open Unity, select both engine empty objects and move them slightly UP (along the Y axis) until they are
        // aligned with the center of gravity.
        float currentThrust = Mathf.Lerp(engineData.minThrust, engineData.maxThrust, throttle);
        float thrustPerEngine = currentThrust / engineExhausts.Length;

        foreach (var exhaust in engineExhausts)
        {
            if (exhaust != null) AddForceAtPosition(exhaust.forward * thrustPerEngine, exhaust.position);
        }
    }

    private void HandleGroundPhysics(float dt, float collisionRadius, out bool isGrounded, out RaycastHit groundHit)
    {
        isGrounded = Physics.Raycast(_transform.position, Vector3.down, out groundHit, collisionRadius + 0.5f, groundMask);

        if (isGrounded)
        {
            float downwardForce = Vector3.Dot(_accumulatedForce, groundHit.normal);
            if (downwardForce < 0) _accumulatedForce -= groundHit.normal * downwardForce;

            if (throttle < 0.05f)
            {
                if (LinearVelocity.sqrMagnitude < 9.0f && AngularVelocity.sqrMagnitude < 0.25f)
                {
                    // Parking Brake (Sleep State)
                    LinearVelocity = Vector3.zero;
                    AngularVelocity = Vector3.zero;
                    _accumulatedForce = Vector3.zero;
                    _accumulatedTorque = Vector3.zero;
                }
                else
                {
                    LinearVelocity = Vector3.Lerp(LinearVelocity, Vector3.zero, 3f * dt);
                    AngularVelocity = Vector3.Lerp(AngularVelocity, Vector3.zero, 3f * dt);
                }
            }
            else
            {
                LinearVelocity *= 0.99f;
            }
        }
    }

    private void IntegratePhysics(float dt)
    {
        LinearVelocity += (_accumulatedForce / mass) * dt;

        Vector3 localTorque = _transform.InverseTransformDirection(_accumulatedTorque);
        Vector3 localAngularAcceleration = new Vector3(
            localTorque.x / inertiaTensor.x,
            localTorque.y / inertiaTensor.y,
            localTorque.z / inertiaTensor.z
        );
        Vector3 angularAcceleration = _transform.TransformDirection(localAngularAcceleration);

        AngularVelocity += angularAcceleration * dt;

        // TODO: Replace with proper Angular Drag later
        AngularVelocity *= 0.95f;

        if (AngularVelocity.sqrMagnitude > 400f) AngularVelocity = AngularVelocity.normalized * 20f;
    }

    private void ApplyMovementAndCollisions(float dt, float collisionRadius, bool isGrounded, RaycastHit groundHit)
    {
        Vector3 movementStep = LinearVelocity * dt;

        if (movementStep.sqrMagnitude > 0.0001f && Physics.SphereCast(_transform.position, collisionRadius, movementStep.normalized, out RaycastHit hit, movementStep.magnitude, groundMask))
        {
            LinearVelocity = Vector3.ProjectOnPlane(LinearVelocity, hit.normal);
            _transform.position = hit.point + hit.normal * collisionRadius;
        }
        else
        {
            _transform.position += movementStep;

            if (isGrounded && LinearVelocity.sqrMagnitude < 100f)
            {
                if (LinearVelocity.y < 0) LinearVelocity = new Vector3(LinearVelocity.x, 0f, LinearVelocity.z);
                _transform.position = new Vector3(_transform.position.x, groundHit.point.y + collisionRadius, _transform.position.z);
            }
        }

        _transform.rotation *= Quaternion.Euler(AngularVelocity * (Mathf.Rad2Deg * dt));
    }

    private void ApplyInputToSurfaces(ControlSurface[] surfaces, float input)
    {
        if (surfaces == null) return;
        for (int i = 0; i < surfaces.Length; i++)
        {
            if (surfaces[i] != null) surfaces[i].targetDeflection = input;
        }
    }

    private float CalculatePitchG()
    {
        Vector3 localVelocity = _transform.InverseTransformDirection(LinearVelocity);
        Vector3 localAngularVel = _transform.InverseTransformDirection(AngularVelocity);
        float radius = Mathf.Approximately(localAngularVel.x, 0.0f) ? float.MaxValue : localVelocity.z / localAngularVel.x;
        float verticalForce = Mathf.Approximately(radius, 0.0f) ? 0.0f : (localVelocity.z * localVelocity.z) / radius;
        return (verticalForce / -9.81f) + (_transform.up.y * (Physics.gravity.y / -9.81f));
    }

    private void OnGUI()
    {
        GUI.Label(_rectSpeed, $"Speed: {LinearVelocity.magnitude * 1.94384f:0.0} knots");
        GUI.Label(_rectThrottle, $"Throttle: {throttle * 100.0f:0.0}%");
        GUI.Label(_rectGLoad, $"G Load: {CalculatePitchG():0.0} G");
        GUI.Label(new Rect(10, 100, 300, 20), $"Engine: {(engineOn ? "ON" : "OFF")}");
    }
}