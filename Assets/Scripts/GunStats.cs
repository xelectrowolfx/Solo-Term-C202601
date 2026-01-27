using UnityEngine;

[CreateAssetMenu]   

public class GunStats : ScriptableObject
{
    public GameObject gunModel;

    [Header("       Gun      ")]

    [Range(1,10)][SerializeField] public int shootDamage;
    [Range(3,1000)][SerializeField] public int shootDist;
    [Range(0.1f,3)][SerializeField] public float shootRate;

    public int ammoCur;
    [Range(5,50)] public int ammoMax;

    public ParticleSystem hitEffect;
    public AudioClip[] ShootSound;
    [Range(0, 1)] public float shootSoundVol;


}
