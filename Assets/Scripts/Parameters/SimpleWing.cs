using UnityEngine;

public class SimpleWing : MonoBehaviour
{
    // TODO: BALANCE FIX ("BARREL ROLL" PROBLEM)
    // If the airplane spins uncontrollably: 
    // 1. Ensure the Rotation of WingLeft is strictly (0,0,0). Do not rotate the empty object!
    // 2. Enable the 'isLeftWing' toggle for the left wing, the script will mirror the physics automatically.
    [Header("Wing Shape")]
    [Tooltip("Check this if it is the LEFT wing. DO NOT rotate the object!")]
    public bool isLeftWing = false;

    [Tooltip("Wing span (length of one wing console)")]
    public float span = 5f;
    [Tooltip("Root chord (width at the fuselage)")]
    public float rootChord = 3.5f;
    [Tooltip("Tip chord (width at the tip)")]
    public float tipChord = 0.8f;
    [Tooltip("Sweep angle along the leading edge"), Range(0f, 75f)]
    public float sweepAngle = 42f;

    [Header("Aerodynamics")]
    public WingCurves wing;
    public float liftMultiplier = 1f;
    public float dragMultiplier = 1f;

    private Transform _transform;
    private Airplane _airplane;
    private Vector3 _liftDirection = Vector3.up;

    public float AngleOfAttack { get; private set; }
    public float LiftCoefficient { get; private set; }
    public float DragCoefficient { get; private set; }
    public float LiftForce { get; private set; }
    public float DragForce { get; private set; }

    public float WingArea => span * (rootChord + tipChord) / 2f;

    private void Awake()
    {
        _transform = transform;
        _airplane = GetComponentInParent<Airplane>();
    }

    private void Start()
    {
        if (_airplane == null) Debug.LogError($"{name}: SimpleWing has no Airplane script!");
        if (wing == null) Debug.LogError($"{name}: SimpleWing has no defined wing curves!");
    }

    private void FixedUpdate()
    {
        if (_airplane == null || wing == null) return;

        float direction = isLeftWing ? -1f : 1f;
        float sweepOffsetZ = Mathf.Tan(sweepAngle * Mathf.Deg2Rad) * span;

        // TODO: BALANCE FIX ("NOSE PITCH UP" PROBLEM)
        // Force is applied here (aeroCenterWorld). If the nose still pulls up heavily during takeoff,
        // simply move the WingLeft and WingRight empty objects slightly BACKWARD (along the Z axis) in the Unity Editor.
        Vector3 aeroCenterLocal = new Vector3((span / 2f) * direction, 0f, -sweepOffsetZ / 2f);
        Vector3 aeroCenterWorld = _transform.TransformPoint(aeroCenterLocal);

        Vector3 pointVelocity = _airplane.GetPointVelocity(aeroCenterWorld);
        Vector3 localVelocity = _transform.InverseTransformDirection(pointVelocity);
        localVelocity.x = 0f;

        if (localVelocity.sqrMagnitude < 0.1f) return;

        AngleOfAttack = Vector3.Angle(Vector3.forward, localVelocity) * -Mathf.Sign(localVelocity.y);
        float absAoA = Mathf.Abs(AngleOfAttack);

        float sweepFactor = Mathf.Cos(sweepAngle * Mathf.Deg2Rad);
        LiftCoefficient = wing.GetLiftAtAOA(absAoA) * sweepFactor;
        DragCoefficient = wing.GetDragAtAOA(absAoA);

        float sqrMagnitude = localVelocity.sqrMagnitude;

        LiftForce = sqrMagnitude * LiftCoefficient * WingArea * liftMultiplier * -Mathf.Sign(localVelocity.y);
        DragForce = sqrMagnitude * DragCoefficient * WingArea * dragMultiplier;

        _liftDirection = Vector3.ProjectOnPlane(_transform.up, pointVelocity).normalized;
        if (_liftDirection == Vector3.zero) _liftDirection = _transform.up;

        Vector3 totalForce = (_liftDirection * LiftForce) + (-pointVelocity.normalized * DragForce);
        _airplane.AddForceAtPosition(totalForce, aeroCenterWorld);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (_airplane != null && Application.isPlaying)
        {
            float direction = isLeftWing ? -1f : 1f;
            float sweepOffsetZ = Mathf.Tan(sweepAngle * Mathf.Deg2Rad) * span;
            Vector3 aeroCenterLocal = new Vector3((span / 2f) * direction, 0f, -sweepOffsetZ / 2f);
            Vector3 aeroCenterWorld = _transform.TransformPoint(aeroCenterLocal);

            Debug.DrawRay(aeroCenterWorld, _liftDirection * LiftForce * 0.0001f, Color.blue);
            Debug.DrawRay(aeroCenterWorld, -_airplane.GetPointVelocity(aeroCenterWorld).normalized * DragForce * 0.0001f, Color.red);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        float direction = isLeftWing ? -1f : 1f;
        float sweepOffsetZ = Mathf.Tan(sweepAngle * Mathf.Deg2Rad) * span;

        Vector3 rootLeadingEdge = new Vector3(0, 0, rootChord / 2f);
        Vector3 rootTrailingEdge = new Vector3(0, 0, -rootChord / 2f);

        Vector3 tipLeadingEdge = new Vector3(span * direction, 0, (rootChord / 2f) - sweepOffsetZ);
        Vector3 tipTrailingEdge = new Vector3(span * direction, 0, tipLeadingEdge.z - tipChord);

        Gizmos.DrawLine(rootLeadingEdge, tipLeadingEdge);
        Gizmos.DrawLine(rootTrailingEdge, tipTrailingEdge);
        Gizmos.DrawLine(rootLeadingEdge, rootTrailingEdge);
        Gizmos.DrawLine(tipLeadingEdge, tipTrailingEdge);

        Vector3 aeroCenter = new Vector3((span / 2f) * direction, 0, -sweepOffsetZ / 2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(aeroCenter, 0.2f);

        Gizmos.matrix = oldMatrix;
    }
#endif
}