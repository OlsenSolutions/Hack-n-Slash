using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    NavMeshAgent agent;
    bool patrolling;
    public Vector3[] patrolTargets;
    private int destPoint;
    bool arrived;
    GameObject player;
    Animator anim;
    EnemyStats enemyStats;
    public bool MagicHit;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        enemyStats = GetComponent<EnemyStats>();
        MagicHit = false;
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(this.gameObject.transform.position, player.gameObject.transform.position);

            if (!MagicHit)
            {
                if (distance < 2f)
                {
                    Follow(true, distance);
                }
                else if (distance >= 2f && distance < 5f)
                {
                    Follow(false, distance);
                }
                else if (distance >= 5f)
                {
                    anim.SetFloat("playerFar", distance);
                    anim.SetFloat("Speed", agent.velocity.magnitude);

                }
            }
            else
            {
                if (distance < 2f)
                {
                    Follow(true, distance);
                }
                else if (distance >= 2f)
                {
                    anim.SetFloat("playerFar", distance);
                    Follow(false, distance);
                }
            }

        }
        else
            player = GameObject.FindGameObjectWithTag("Player");

        if (agent.pathPending)
        {
            return;
        }

        if (patrolling)
        {
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                if (!arrived)
                {
                    anim.SetFloat("Speed", 0f);
                    arrived = true;
                    StartCoroutine("GoToNextPoint");
                }
            }

            else
            {
                arrived = false;
            }
        }
        else
        {
            StartCoroutine("GoToNextPoint");
        }



    }

    IEnumerator GoToNextPoint()
    {
        var random = new System.Random();
        if (patrolTargets.Length == 0)
        {
            yield break;
        }
        patrolling = true;
        yield return new WaitForSeconds(2f);
        arrived = false;
        int x = random.Next(0, patrolTargets.Length - 1);
        destPoint = x;
        if (agent.isActiveAndEnabled != false)
            agent.destination = patrolTargets[destPoint];
        anim.SetFloat("Speed", enemyStats.enemyDefinition.normalSpeed);

        destPoint = (destPoint + 1) % patrolTargets.Length;
    }

    void Follow(bool playerIsNear, float distance)
    {
        LookAt();
        agent.speed = enemyStats.enemyDefinition.normalSpeed;
        anim.SetBool("PlayerIsNear", playerIsNear);
        anim.SetFloat("Speed", agent.velocity.magnitude);
        agent.destination = player.gameObject.transform.position;
        anim.SetFloat("playerFar", distance);
    }

    void LookAt()
    {
        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
    }
}
