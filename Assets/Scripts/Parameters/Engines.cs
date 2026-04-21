using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "Engine_RD33", menuName = "ScriptableObjects/Engines/Engine_RD33")]
public class Engines : ScriptableObject {
    public float maxThrust;
    public float minThrust;
    public float maxHealth = 100;
    public float Health = 100;
}
