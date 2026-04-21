using UnityEngine;

public class ControlSurface : MonoBehaviour
{
    [Header("Deflection")]
    [Tooltip("Deflection with max positive input."), Range(0, 90)]
    public float max = 15f;

    [Tooltip("Deflection with max negative input"), Range(0, 90)]
    public float min = 15f;

    [Tooltip("Speed of the control surface deflection.")]
    public float moveSpeed = 90f;

    [Tooltip("Requested deflection of the control surface normalized to [-1, 1]. "), Range(-1, 1)]
    public float targetDeflection = 0f;

    [Header("Speed Stiffening")]
    [Tooltip("Wing to use for deflection forces. Deflection limited based on airspeed will not function without a reference wing.")]
    [SerializeField] private SimpleWing wing = null;

    [Tooltip("How much force the control surface can exert. The lower this is, the more the control surface stiffens with speed.")]
    public float maxTorque = 6000f;

    private Rigidbody _rigid = null;
    private Transform _transform;
    private Quaternion _startLocalRotation = Quaternion.identity;
    private float _currentAngle = 0f;

    private void Awake()
    {
        _transform = transform;
        if (wing != null)
            _rigid = GetComponentInParent<Rigidbody>();
    }

    private void Start()
    {
        _startLocalRotation = _transform.localRotation;
    }

    private void FixedUpdate()
    {
        float targetAngle = targetDeflection > 0f ? targetDeflection * max : targetDeflection * min;

        if (_rigid != null && wing != null && _rigid.velocity.sqrMagnitude > 1f)
        {
            float torqueAtMaxDeflection = _rigid.velocity.sqrMagnitude * wing.WingArea;
            float maxAvailableDeflection = Mathf.Asin(maxTorque / torqueAtMaxDeflection) * Mathf.Rad2Deg;

            if (!float.IsNaN(maxAvailableDeflection))
                targetAngle *= Mathf.Clamp01(maxAvailableDeflection);
        }

        _currentAngle = Mathf.MoveTowards(_currentAngle, targetAngle, moveSpeed * Time.fixedDeltaTime);

        
        _transform.localRotation = _startLocalRotation * Quaternion.Euler(_currentAngle, 0f, 0f);
    }
}