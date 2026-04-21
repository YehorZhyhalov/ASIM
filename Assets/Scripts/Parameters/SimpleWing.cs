using UnityEngine;

public class SimpleWing : MonoBehaviour
{
    [Tooltip("Size of the wing. The bigger the wing, the more lift it provides.")]
    public Vector2 dimensions = new Vector2(5f, 2f);

    [Tooltip("When true, wing forces will be applied only at the center of mass.")]
    public bool applyForcesToCenter = false;

    [Tooltip("Lift coefficient curve.")]
    public WingCurves wing;
    [Tooltip("The higher the value, the more lift the wing applies at a given angle of attack.")]
    public float liftMultiplier = 1f;
    [Tooltip("The higher the value, the more drag the wing incurs at a given angle of attack.")]
    public float dragMultiplier = 1f;

    // Кешируем ссылки для производительности
    private Transform _transform;
    private Rigidbody _rigid;

    private Vector3 _liftDirection = Vector3.up;

    public float AngleOfAttack { get; private set; }
    public float LiftCoefficient { get; private set; }
    public float DragCoefficient { get; private set; }
    public float LiftForce { get; private set; }
    public float DragForce { get; private set; }

    public float WingArea => dimensions.x * dimensions.y;

    public Rigidbody Rigidbody
    {
        set { _rigid = value; }
    }

    private void Awake()
    {
        _transform = transform;
        _rigid = GetComponentInParent<Rigidbody>();
    }

    private void Start()
    {
        if (_rigid == null)
            Debug.LogError($"{name}: SimpleWing has no rigidbody on self or parent!");

        if (wing == null)
            Debug.LogError($"{name}: SimpleWing has no defined wing curves!");
    }

    
    private void OnValidate()
    {
        dimensions.x = Mathf.Max(0.01f, dimensions.x);
        dimensions.y = Mathf.Max(0.01f, dimensions.y);
    }

    private void Update()
    {
        
#if UNITY_EDITOR
        if (_rigid != null)
        {
            Debug.DrawRay(_transform.position, _liftDirection * LiftForce * 0.0001f, Color.blue);
            Debug.DrawRay(_transform.position, -_rigid.velocity.normalized * DragForce * 0.0001f, Color.red);
        }
#endif
    }

    private void FixedUpdate()
    {
        if (_rigid == null || wing == null) return;

        Vector3 forceApplyPos = applyForcesToCenter ? _rigid.transform.TransformPoint(_rigid.centerOfMass) : _transform.position;

        Vector3 localVelocity = _transform.InverseTransformDirection(_rigid.GetPointVelocity(_transform.position));
        localVelocity.x = 0f;

        // Angle of attack is used as the look up for the lift and drag curves.
        AngleOfAttack = Vector3.Angle(Vector3.forward, localVelocity) * -Mathf.Sign(localVelocity.y);

        // Используем модуль угла для кривых, если они несимметричны
        float absAoA = Mathf.Abs(AngleOfAttack);

        LiftCoefficient = wing.GetLiftAtAOA(absAoA);
        DragCoefficient = wing.GetDragAtAOA(absAoA);

        float sqrMagnitude = localVelocity.sqrMagnitude;

        // Calculate lift/drag.
        LiftForce = sqrMagnitude * LiftCoefficient * WingArea * liftMultiplier * -Mathf.Sign(localVelocity.y);
        DragForce = sqrMagnitude * DragCoefficient * WingArea * dragMultiplier;

        // Lift is always perpendicular to air flow.
        _liftDirection = Vector3.Cross(_rigid.velocity, _transform.right).normalized;
        _rigid.AddForceAtPosition(_liftDirection * LiftForce, forceApplyPos, ForceMode.Force);

        
        if (_rigid.velocity.sqrMagnitude > 0.1f)
        {
            _rigid.AddForceAtPosition(-_rigid.velocity.normalized * DragForce, forceApplyPos, ForceMode.Force);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(dimensions.x, 0f, dimensions.y));
        Gizmos.matrix = oldMatrix;
    }
#endif
}