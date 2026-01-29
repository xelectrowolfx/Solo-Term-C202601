using System.Collections;
using UnityEditor.UIElements;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;


public class PlayerController : MonoBehaviour, IDamage, IPickup
{
    [Header("       Components      ")]
    [SerializeField] CharacterController controller;
    [SerializeField] AudioSource aud;
    [SerializeField] RigBuilder rig;
    [SerializeField] Animator anim;


    [Header("       Audio Clips      ")]
    [SerializeField] AudioClip[] footSteps;
    [Range(0f, 1f)][SerializeField] float FootSteps_Volume = .25f;
    [SerializeField] AudioClip[] Hurt;
    [Range(0f, 1f)][SerializeField] float Hurt_Volume = .25f;
    [SerializeField] AudioClip[] Dying;
    [Range(0f, 1f)][SerializeField] float Dying_Volume = .25f;

    [Header("       Stats      ")]
    [Range(1, 10)] [SerializeField] int HP;
    [Range(1, 10)][SerializeField] int Shield;
   // [Range(1, 10)][SerializeField] int ShieldRegenRate;
    [Range(1, 10)][SerializeField] int speed;
    [Range(2, 5)][SerializeField] int sprintMod;
    [Range(1, 20)][SerializeField] int jumpSpeed;
    [Range(1, 3)][SerializeField] int jumpMax;

    [Header("       Physics      ")]
    [Range(15, 40)][SerializeField] int gravity;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] bool DrawDebug;

    [Header("       Gun      ")]
    [SerializeField] List<GunStats> gunList = new List<GunStats>();
    [SerializeField] GameObject gunmodel;
    [Range(1, 10)][SerializeField] int shootDamage;
    [Range(3, 1000)][SerializeField] int shootDist;
    [Range(0.1f, 3)][SerializeField] float shootRate;


    Vector3 moveDir;
    Vector3 playerVel;

    float shootTimer;
    //float shieldTimer;

    int JumpCount;
    int HPOrig;
    int ShieldOrig;
    int gunListPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        ShieldOrig = Shield;
        spawnPlayer();

    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
    }
    void movement()
    {
        selectGun();
        if (DrawDebug)
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);
        }
        

        shootTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            JumpCount = 0;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }
        if (Input.GetButtonDown("Reload") && gunList.Count > 0) {
            gunList[gunListPos].ammoCur = gunList[gunListPos].ammoMax;
        }
        
        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0 && shootTimer >= shootRate)
        {
            shoot();
        }
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);
        jump();
        controller.Move(playerVel * Time.deltaTime);
       
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && JumpCount < jumpMax)
        {
            playerVel.y = jumpSpeed;
            JumpCount++;

        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }


    void shoot()
    {
        shootTimer = 0;
        gunList[gunListPos].ammoCur--;
        aud.PlayOneShot(gunList[gunListPos].ShootSound[Random.Range(0, gunList[gunListPos].ShootSound.Length)], gunList[gunListPos].shootSoundVol);
        if (gunList[gunListPos].Bullet != null)
        {
            Transform Shoot_Pos = gunList[gunListPos].gunModel.transform.Find("Shoot_Pos");
            Debug.Log(Shoot_Pos);
           Instantiate(gunList[gunListPos].Bullet, Shoot_Pos.position, Quaternion.LookRotation(Camera.main.transform.forward));
        }
        else
        {
            RaycastHit Hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out Hit, shootDist, ~ignoreLayer))
            {
                Debug.Log(Hit.collider.name);

                Instantiate(gunList[gunListPos].hitEffect, Hit.point, Quaternion.identity);
                


                IDamage dmg = Hit.collider.GetComponent<IDamage>();
                if (dmg != null)
                {
                    dmg.takeDamage(shootDamage);
                }

            }
        }
     
    }

    public void takeDamage(int amount)
    {
        int reduced_dmg = amount;
        if (Shield > 0 && Shield > amount)
        {
            Shield -= amount;
            StartCoroutine(FlashBlue());
            reduced_dmg = 0;
        }
        else if (Shield > 0)
        {
            reduced_dmg = amount - Shield;
            Shield = 0;
            StartCoroutine(FlashBlue());

        }
        if (reduced_dmg > 0)
        {
            HP -= reduced_dmg;
            StartCoroutine(FlashRed());
            
            //isdead?
            if (HP <= 0)
            {
                GameManager.instance.youLose();

            }
        }
        UpdatePlayerUI();

    }
    IEnumerator FlashBlue()
    {
        GameManager.instance.DamageScreenShield.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.DamageScreenShield.SetActive(false);
    }
    IEnumerator FlashRed()
    {
        GameManager.instance.DamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.DamageScreen.SetActive(false);
    }

    //IEnumerator ShieldBroken()
    //{
    //    yield return new WaitForSeconds(3f);

    //}
    public void UpdatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
        GameManager.instance.playerShieldBar.fillAmount = (float)Shield / ShieldOrig;
    }

    public void getGunStats(GunStats gun)
    {
        if (gunList.Contains(gun))
        {
            gunList.Remove(gun);
            gunList.Add(gun);
        }
        else
        {
            gunList.Add(gun);
        }
        gunListPos = gunList.Count - 1;

        changeGun();

    }
    void changeGun()
    {
        anim.enabled = false;
        rig.enabled = true;
        shootDamage = gunList[gunListPos].shootDamage;
        shootDist = gunList[gunListPos].shootDist;
        shootRate = gunList[gunListPos].shootRate;
        gunmodel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunmodel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void selectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count -1)
        {
            gunListPos++;
            changeGun();

        }

        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
        {
            gunListPos--;
            changeGun();
        }

        
    }

    public void spawnPlayer()
    {
        controller.transform.position = GameManager.instance.playerSpawnPos.transform.position;
        HP = HPOrig;
        Shield = ShieldOrig;
        UpdatePlayerUI();
        Physics.SyncTransforms();
    }
}
