using UnityEngine;

public class Radar : MonoBehaviour
{

    public GameObject enemy;

    //Radius
    public float distance = 2f;
    
    public float Angle = 360f;

    public int count = 15;


    void Start()
    {
    
        Vector3 point = transform.position;

        Angle = Angle * Mathf.Rad2Deg;

        for (int i = 1; i <= count; i++) {
        
            float _z = transform.position.z + Mathf.Cos(Angle / count * i) * distance;

            
            float _x = transform.position.x + Mathf.Sin(Angle / count * i) * distance;
        
            point.x = _x;
            point.z = _z;

            Instantiate(enemy, point, Quaternion.identity);
         } 


    }

}
