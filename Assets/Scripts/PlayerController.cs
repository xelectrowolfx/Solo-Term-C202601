using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour, IDamage
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

    [Header("       Gun      ")]
    [SerializeField] bool DrawDebug;
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    Vector3 moveDir;
    Vector3 playerVel;

    float shootTimer;

    int JumpCount;
    int HPOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        updatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
    }
    void movement()
    {
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
        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
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

        RaycastHit Hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out Hit, shootDist))
        {
            Debug.Log(Hit.collider.name);
            IDamage dmg = Hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }

        }
        
   

    }
    IEnumerator FlashRed()
    {
        GameManager.instance.gameFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.gameFlash.SetActive(false);
    }
    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();
        StartCoroutine(FlashRed());
        //isdead?
        if (HP <= 0)
        {
            GameManager.instance.youLose();
        }
    }

    public void updatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
    }

    }

