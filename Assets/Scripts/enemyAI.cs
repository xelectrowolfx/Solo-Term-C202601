using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using Color = UnityEngine.Color;

public class enemyAI : MonoBehaviour, IDamage

{
    [Header("------ Enemy Dependancies ------")]
    [SerializeField] Renderer model;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform headPos;
    [SerializeField] GameObject dropItem;

    [Header("------ Enemy STATS ------")]
    [Range(1, 10)][SerializeField] int HP = 5;
    [Range(0f, 3.0f)][SerializeField] float shootRate = 1f;

    [Header("------ AI Dependancies ------")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animation_State_Controller controller;

    [Header("------ AI Stats ------")]
    [Range(20,100)][SerializeField] int faceTargetSpeed = 50;
    [Range(1.1f,3.0f)][SerializeField] float AgentSprintMod = 1.2f;
    [Range(20,50)][SerializeField] int AgentSprintDistance = 30;
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

    //float distance;

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
   


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        GameManager.instance.updateGameGoal(1);
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
        //Debug.Log("is Walking: " + controller.GetIsWalking());
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
        //if (distance <= stopDist)
        //{
        //    //if we are walking or performing other actions we stop.
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
        AI();
    }

    void shoot()
    {
        shootTimer = 0;
        controller.SetIsFiring(true);
        Instantiate(bullet, shootPos.position, transform.rotation);
        controller.SetIsFiring(false);

    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerdir.x, transform.position.y, playerdir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
    public void takeDamage(int amount)
    {
        HP -= amount;
        EnemySpotted();
        if(HP <= 0)
        {
            GameManager.instance.updateGameGoal(-1);
            if (dropItem != null)
            {
                Instantiate(dropItem, transform.position, transform.rotation);
            }
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }
    private void EnemySpotted()
    {
        Alerted = true;
        LastKnownLoc = GameManager.instance.player.transform.position;
        sprint();

    }
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;

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
                EnemySpotted();
                

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
        //Get Player Direction
        playerdir = GameManager.instance.player.transform.position - transform.position;

        //Direct Agent to Player
        //agent.SetDestination(GameManager.instance.player.transform.position);

        //Get Distance
        //distance = agent.remainingDistance;

        shootTimer += Time.deltaTime;

        if (agent.remainingDistance < 0.1f + Random.value)
        {
            RoamTimer += Time.deltaTime;
            SearchTimer += Time.deltaTime;
        }
        
        //check if we can see player

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

        //Behavior Tree
        //if (distance > stopDist && distance != 0)
        //{
        //    controller.SetIsAiming(false);
        //    if (distance >= AgentSprintDistance )
        //    {
        //        controller.SetIsRunning(true);
        //        controller.SetIsWalking(false);
        //        if(agent.speed != speedOrig * AgentSprintMod)
        //        {
        //            agent.speed = agent.speed * AgentSprintMod;
        //        }

        //    }
        //    else
        //    {
        //        controller.SetIsWalking(true);
        //        controller.SetIsRunning(false);
        //        agent.speed = speedOrig;
        //    }
        //}
        //if (distance <= stopDist)
        //{
        //    //if we are walking or performing other actions we stop.
        //    if (controller.GetIsWalking() || controller.GetIsRunning())
        //    {
        //        controller.SetIsWalking(false);
        //        controller.SetIsRunning(false);
        //    }



        //    //we are in stopping distance so we stop, and look at player to shoot.
        //    faceTarget();
        //    //Debug.Log(distance + "m to player.");

        //    controller.SetIsAiming(true);
        //    if(shootTimer >= shootRate)
        //    {
        //        controller.SetIsFiring(true);

        //        shoot();

        //        controller.SetIsFiring(false);
        //    }


        //}
    }
}
