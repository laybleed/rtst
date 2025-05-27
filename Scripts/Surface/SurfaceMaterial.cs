using UnityEngine;

[CreateAssetMenu(fileName = "NewSurfaceMaterial", menuName = "Audio/Surface Material")]
public class SurfaceMaterial : ScriptableObject
{
    public string materialName;
    public float reflectionCoefficient = 0.5f; 
    public float absorptionCoefficient = 5e-3f; 
    public float penetrationResistance = 20f; 

    public float pitchModifier = 1.0f; 
    public float reverbTail = 0.5f; 
    public float lowPassCutoff = 22000f; 
}
