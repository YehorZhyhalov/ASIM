using UnityEngine;

public class Jet : MonoBehaviour
{
    [Header("Landing Gear")]
    public GameObject LandingGearFront;
    public GameObject LandingGearBackRight;
    public GameObject LandingGeearBackLeft;

    [Header("Visuals")]
    public Material engineMat;

    private Airplane _airplane;

    void Start()
    {
        _airplane = GetComponent<Airplane>();
    }

    void Update()
    {
        // 1. Уборка/Выпуск шасси на кнопку G
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (LandingGearFront != null) LandingGearFront.SetActive(!LandingGearFront.activeSelf);
            if (LandingGearBackRight != null) LandingGearBackRight.SetActive(!LandingGearBackRight.activeSelf);
            if (LandingGeearBackLeft != null) LandingGeearBackLeft.SetActive(!LandingGeearBackLeft.activeSelf);
        }

        if (_airplane != null)
        {
            // 2. Включение/Выключение двигателей на пробел
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _airplane.engineOn = !_airplane.engineOn;
                Debug.Log(_airplane.engineOn ? "Engine Start" : "Engine Stop");
            }

            // 3. Эффект пламени из сопла (читает текущую тягу из Airplane)
            if (engineMat != null)
            {
                engineMat.SetFloat("_EnginePower", _airplane.throttle);
            }
        }
    }
}