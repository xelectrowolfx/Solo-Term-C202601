using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using Color = UnityEngine.Color;

public class enemyAI : MonoBehaviour, IDamage

{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animation_State_Controller controller;
    [SerializeField] Renderer model;
    [SerializeField] Transform shootPos;
    
    [SerializeField] GameObject bullet;

    [SerializeField] int HP;
    [SerializeField] float shootRate;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] float AgentSprintMod = 1.2f;
   

    Color colorOrig;
    float shootTimer;
    Vector3 playerdir;
    float speedOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        GameManager.instance.updateGameGoal(1);
        speedOrig = agent.speed;
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;

        playerdir = GameManager.instance.player.transform.position - transform.position;
        
        agent.SetDestination(GameManager.instance.player.transform.position);

        if (shootTimer >= shootRate)
        {
            shoot();
            controller.SetIsFiring(false);
        }
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            faceTarget();
            controller.SetIsAiming(true);
            controller.SetIsWalking(false);
        }
        else if(agent.remainingDistance >= agent.stoppingDistance * 2)
        {
            agent.speed = agent.speed * AgentSprintMod;
            controller.SetIsRunning(true);
        }
        else
        {
            controller.SetIsAiming(false);
            controller.SetIsWalking(true);
        }
  
    }

    void shoot()
    {
        shootTimer = 0;
        controller.SetIsFiring(true);
        Instantiate(bullet, shootPos.position, transform.rotation);

    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerdir.x, transform.position.y, playerdir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
    public void takeDamage(int amount)
    {
        HP -= amount;

        if(HP <= 0)
        {
            GameManager.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;

    }
}
