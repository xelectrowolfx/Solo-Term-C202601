using System.Collections;
using UnityEditor.UIElements;
using UnityEngine;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour, IDamage, IPickup
{
    [Header("       Components      ")]
    [SerializeField] CharacterController controller;

    [Header("       Stats      ")]
    [Range(1, 10)] [SerializeField] int HP;
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

    int JumpCount;
    int HPOrig;
    int gunListPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
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

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(FlashRed());
        UpdatePlayerUI();
        //isdead?
        if(HP <= 0)
        {
            GameManager.instance.youLose();

        }
    }

    IEnumerator FlashRed()
    {
        GameManager.instance.DamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.DamageScreen.SetActive(false);
    }

    public void UpdatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
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
        UpdatePlayerUI();
        Physics.SyncTransforms();
    }
}
