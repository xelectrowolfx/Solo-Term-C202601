using UnityEngine;

public class pickupGuns : MonoBehaviour
{
    [SerializeField] GunStats gun;


    private void OnTriggerEnter(Collider other)
    {
        IPickup pik = other.GetComponent<IPickup>();

        if(pik != null)
        {
            gun.ammoCur = gun.ammoMax;
            pik.getGunStats(gun);
            Destroy(gameObject);

        }
    }
}


