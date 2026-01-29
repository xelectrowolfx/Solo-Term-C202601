using System.Collections;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using Color = UnityEngine.Color;

public class enemyAI : MonoBehaviour, IDamage, IFootstep

{
    [Header("------ Enemy Dependancies ------")]
    [SerializeField] Renderer model;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform headPos;
    [SerializeField] GameObject dropItem;
    
    [SerializeField] AudioClip gunfire;
    [Range(0f, 1f)][SerializeField] float Gun_Volume = .5f;
    [SerializeField] AudioClip[] footSteps;
    [Range(0f, 1f)][SerializeField] float FootSteps_Volume = .25f;
    [SerializeField] AudioClip[] Hurt;
    [Range(0f, 1f)][SerializeField] float Hurt_Volume = .25f;
    [SerializeField] AudioClip[] Dying;
    [Range(0f, 1f)][SerializeField] float Dying_Volume = .25f;
    [SerializeField] AudioClip[] PlayerSpotted;
    [Range(0f, 1f)][SerializeField] float PlayerSpotted_Volume = .25f;
    
    [Header("------ Enemy STATS ------")]
    [Range(1, 10)][SerializeField] int HP = 5;
    [Range(0f, 3.0f)][SerializeField] float shootRate = 1f;

    [Header("------ AI Dependancies ------")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animation_State_Controller controller;
    [SerializeField] RigBuilder Ik_Rig;

    [Header("------ AI Stats ------")]
    [Range(20,100)][SerializeField] int faceTargetSpeed = 50;
    [Range(1.1f,3.0f)][SerializeField] float AgentSprintMod = 2f;
    [Range(10, 50)][SerializeField] int AgentAlertedSearchDistance = 10;
    [Range(5, 120)][SerializeField] int AgentAlertTime = 30;
    [Range(1, 10)][SerializeField] int AgentAlertPauseTime = 2;
    [SerializeField] int FOV;
    [SerializeField] LayerMask IgnoreLayer;

    [SerializeField] int RoamDist;
    [SerializeField] int RoamPauseTime;

    //Player
    float angleToPlayer;
    Vector3 playerdir;
    Vector3 LastKnownLoc;

    //enemy
    public Vector3 AI_Forwards;
    public Vector3 AI_Move_Dir;
    public Vector3 AI_Cur_Speed;


    //Booleans
    bool Alerted;
    bool playerinTrigger;

    //Initial Vars
    Vector3 startingPos;
    float speedOrig;
    Color colorOrig;
    float stoppingDistOrig;


    //Timers
    float RoamTimer;
    float shootTimer;
    float AlertedTimer;
    float SearchTimer;
    private bool Alive = true;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        //GameManager.instance.updateGameGoal(1);
        speedOrig = agent.speed;
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;
    }
    void idle()
    {
        controller.SetIsAiming(false);
        controller.SetIsRunning(false);
        controller.SetIsWalking(false);
        controller.SetIsFiring(false);
    }
    void walk()
    {
        controller.SetIsWalking(true);
        controller.SetIsRunning(false);
        agent.speed = speedOrig;
    }
    void sprint()
    {
        controller.SetIsRunning(true);
        controller.SetIsWalking(false);
        controller.SetIsAiming(false);

        if (agent.speed != speedOrig * AgentSprintMod)
        {
            agent.speed = speedOrig * AgentSprintMod;
        }
    }

    void aim()
    {
        
        if (controller.GetIsWalking() || controller.GetIsRunning())
        {
            controller.SetIsWalking(false);
            controller.SetIsRunning(false);
        }
        controller.SetIsAiming(true);

    }
    // Update is called once per frame
    void Roam()
    {
        RoamTimer = 0;
        agent.stoppingDistance = 0;
        Vector3 ranPos = Random.insideUnitSphere * RoamDist;
        ranPos += startingPos;
        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, RoamDist, 1);
        agent.SetDestination(hit.position);
        walk();
    }

    void AlertedSearch()
    {
        SearchTimer = 0;
        agent.stoppingDistance = 0;
        Vector3 ranpos = Random.insideUnitSphere * AgentAlertedSearchDistance;
        ranpos += LastKnownLoc;
        NavMeshHit hit;
        NavMesh.SamplePosition(ranpos, out hit, AgentAlertedSearchDistance, 1);
        agent.SetDestination(hit.position);
        sprint();
    }

    void CheckSearch()
    {
        if(AlertedTimer < AgentAlertTime && Alerted)
        {
            if (agent.remainingDistance < 0.01f && SearchTimer >= AgentAlertPauseTime && Alerted)
            {
                AlertedSearch();
            }
            else if (agent.remainingDistance < 0.01f && SearchTimer < AgentAlertPauseTime && Alerted)
            {
                idle();
            }
        }
        else
        {
            Alerted = false;
            AlertedTimer = 0;
        }
        

    }
    void checkRoam()
    {
        if (agent.remainingDistance < 0.01f && RoamTimer >= RoamPauseTime && !Alerted)
        {
           Roam();
        }
        else if (agent.remainingDistance < 0.01f && RoamTimer< RoamPauseTime)
        {
           idle();
        }
      
    }
    void Update()
    {

        if (Alive)
        {
          
            AI();
        }
        
    }

    void shoot()
    {
        shootTimer = 0;
        controller.SetIsFiring(true);
        Instantiate(bullet, shootPos.position, transform.rotation);
        AudioSource.PlayClipAtPoint(gunfire, shootPos.position, Gun_Volume);
        controller.SetIsFiring(false);

    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerdir.x, transform.position.y, playerdir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
    public void takeDamage(int amount)
    {
        if (Alive)
        {
            HP -= amount;
            if (!Alerted)
            {
                EnemySpotted();
            }
            else
            {
                LastKnownLoc = GameManager.instance.player.transform.position;
            }

            if (HP <= 0)
            {
                GameManager.instance.updateGameGoal(-1);
                if (dropItem != null)
                {
                    Instantiate(dropItem, transform.position, transform.rotation);
                }
                Ik_Rig.enabled = false;
                controller.SetPlayDeath();
                agent.enabled = false;
                AudioSource.PlayClipAtPoint(Dying[Random.Range(0,Dying.Length)], transform.position, Dying_Volume);
                Alive = false;
               StartCoroutine( DestroyBody());
            }
            else
            {
                StartCoroutine(flashRed());
                AudioSource.PlayClipAtPoint(Hurt[Random.Range(0, Hurt.Length)], transform.position, Hurt_Volume);
            }
        }
        
    }
    private void EnemySpotted()
    {
        Alerted = true;
        LastKnownLoc = GameManager.instance.player.transform.position;
        agent.SetDestination(LastKnownLoc);
        sprint();
        AudioSource.PlayClipAtPoint(PlayerSpotted[Random.Range(0, PlayerSpotted.Length)], transform.position, PlayerSpotted_Volume);

    }
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;

    }

    IEnumerator DestroyBody()
    {
        yield return new WaitForSeconds(GameManager.instance.BodyCleanUpTime);
        Destroy(gameObject);
    }
    bool CanSeePlayer()
    {
        playerdir = GameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerdir, transform.forward);
        Debug.DrawRay(headPos.position, playerdir);
        RaycastHit hit;

        if (Physics.Raycast(headPos.position, playerdir, out hit, float.MaxValue, ~IgnoreLayer))
        {
            if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {
                agent.SetDestination(GameManager.instance.player.transform.position);
                if (!Alerted)
                {
                    EnemySpotted();
                }
                else
                {
                    LastKnownLoc = GameManager.instance.player.transform.position;
                }



                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();

                    aim();
                }

                if (shootTimer >= shootRate && agent.remainingDistance <= agent.stoppingDistance)
                {
                    shoot();
                }

                agent.stoppingDistance = stoppingDistOrig;
                return true;
            }
           
        }

        agent.stoppingDistance = 0;
        return false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerinTrigger = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerinTrigger = false;
            agent.stoppingDistance = 0;
        }
    }
    private void AI()
    {
   
        playerdir = GameManager.instance.player.transform.position - transform.position;
        AI_Forwards = transform.forward;
        AI_Move_Dir = agent.velocity.normalized;
        AI_Cur_Speed = agent.velocity;
        shootTimer += Time.deltaTime;



        if (agent.remainingDistance < 0.1f + Random.value)
        {
            RoamTimer += Time.deltaTime;
            SearchTimer += Time.deltaTime;
        }
        
  

        if (playerinTrigger && !CanSeePlayer())
        {
            if (Alerted)
            {
                CheckSearch();
                AlertedTimer += Time.deltaTime;
            }
            else
            {
                checkRoam();
            }
                
        }
        else if (!playerinTrigger)
        {
            if (Alerted)
            {
                CheckSearch();
            }
            else
            {
                checkRoam();

            }
        }

    }

    public void FootStepEvent(Vector3 Pos)
    {
        AudioSource.PlayClipAtPoint(footSteps[Random.Range(0, footSteps.Length)], Pos, FootSteps_Volume);
    }
}
