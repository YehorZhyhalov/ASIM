using AmplifyShaderEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class Jet : MonoBehaviour
{
    [SerializeField] private AnimationCurve StartEngine;


    [SerializeField] private Engines EngineRD33;
    

    public GameObject LandingGearFront;
    public GameObject LandingGearBackRight;
    public GameObject LandingGeearBackLeft;

    public Transform engineLeft;
    public Transform engineRight;


    public bool engineOn = false;
    public Material engineMat;
    public float throttleSpeed = 1f;




    [Range(0f, 1f)] public float throttle = 0f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {   
        
        UpdateEngineControll();
        UpdateLandeingGear();
        


        if (!engineOn) { return; }
        EngineUpdate();



    }
    void FixedUpdate()
    {
        
    }



    void UpdateLandeingGear() {

        if (Input.GetKeyDown(KeyCode.G)) {

            LandingGearFront.SetActive(!LandingGearFront.activeSelf);
            LandingGearBackRight.SetActive(!LandingGearBackRight.activeSelf);
            LandingGeearBackLeft.SetActive(!LandingGeearBackLeft.activeSelf);

        }
    
    
    
    }




    void UpdateEngineControll() {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            throttle += throttleSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            throttle -= throttleSpeed * Time.deltaTime;
        }
        throttle = Mathf.Clamp01(throttle);
        if (Input.GetKeyUp(KeyCode.Space))
        {
            engineOn = !engineOn;
            Debug.Log(engineOn ? "Engine Start" : "Engine Stop");

        }

    }

    

    void EngineUpdate()
    {


        // Engine system
        float thrustLeft = EngineRD33.maxThrust * throttle;
        float thrustRight = EngineRD33.maxThrust * throttle;

        //Vector3 forceLeft = engineLeft.forward * thrustPerEngine * Time.deltaTime;
        Vector3 forceLeft = Vector3.forward * thrustLeft;


        //LeftEngine
        rb.AddRelativeForce(forceLeft * Time.deltaTime, ForceMode.Force);
        Debug.DrawRay(transform.position, transform.forward * (thrustLeft * 0.001f), Color.blue);
        Debug.Log("Engine force: " + forceLeft);

        Vector3 forceRight = Vector3.forward * thrustRight;
        //RightEngine
        rb.AddRelativeForce(forceRight * Time.deltaTime, ForceMode.Force);
        Debug.DrawRay(transform.position, transform.forward * (thrustRight * 0.001f), Color.blue);
        Debug.Log("Engine force: " + forceRight);

        //VFX
        engineMat.SetFloat("_EnginePower", throttle);



    }





}
