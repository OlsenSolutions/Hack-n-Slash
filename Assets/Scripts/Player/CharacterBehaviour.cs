using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]

public class CharacterBehaviour : MonoBehaviour
{
    public CharacterStats characterDefinition;
    NavMeshAgent agent;
    RaycastHit infoHit = new RaycastHit();
    private float InitialTouch;
    private float nearEnemy;
    public bool isEnemy;
    Animator anim;
    private GameObject actualEnemy;

    public LayerMask clickableLayer;

    public GameObject fireBallPrefab;
    public GameObject fireballHolder;

    void Awake()
    {

    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterDefinition = GetComponent<CharacterStats>();
        isEnemy = false;
        InvokeRepeating("AddHealthAndMana", 1f, 1f);
    }
    void AddHealthAndMana()
    {
        if (characterDefinition.characterDefinition.currentHealth < characterDefinition.characterDefinition.maxHealth)
            characterDefinition.ApplyHealth(characterDefinition.characterDefinition.charRenevalPoints);
        if (characterDefinition.characterDefinition.currentMana < characterDefinition.characterDefinition.maxMana)
            characterDefinition.ApplyMana(characterDefinition.characterDefinition.charRenevalPoints);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (characterDefinition.characterDefinition.currentMana >= characterDefinition.characterDefinition.currentMagicDamage / 2)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(ray, out hit))
                {
                    EventManager.TriggerEvent(EventManager.MagicUsed);

                    GameObject fireball = Instantiate(fireBallPrefab, transform) as GameObject;
                    var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var dir = hit.point - transform.position;
                    dir = new Vector3 (dir.x, 0, dir.z);
                    fireball.GetComponent<Fireball>().Damage = characterDefinition.characterDefinition.currentMagicDamage;
                    Rigidbody rb = fireball.GetComponent<Rigidbody>();
                    rb.velocity = dir;
                    fireball.transform.parent = fireballHolder.transform;
                    characterDefinition.TakeMana(characterDefinition.characterDefinition.currentMagicDamage / 2);
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 500, clickableLayer.value))
            {
                Move(ray, hit);
            }
        }
        if (isEnemy == true && agent.remainingDistance < 2f)
        {
            anim.SetBool("isPlayerAttacking", false);
            anim.SetBool("Attack", true);
            this.gameObject.transform.LookAt(actualEnemy.transform);
        }

        if (characterDefinition.characterDefinition.currentHealth <= 0)
        {
            anim.SetBool("Dead", true);
            GetComponent<NavMeshAgent>().enabled = false;

        }

        anim.SetFloat("PlayerSpeed", agent.velocity.magnitude);

    }

    void Move(Ray ray, RaycastHit hit)
    {
        if (hit.collider.gameObject.tag == "Enemy" || hit.collider.gameObject.tag == "EnemyWeapon")
        {
            actualEnemy = hit.collider.gameObject;
            anim.SetBool("isPlayerAttacking", true);
            agent.speed = 7f;
            isEnemy = true;

            if (Physics.Raycast(ray.origin, ray.direction, out infoHit))
            {
                agent.stoppingDistance = 2f;
                agent.destination = infoHit.point;
            }

        }
        else if (hit.collider.gameObject.tag == "BoxCollective")
        {
            actualEnemy = hit.collider.gameObject;
            anim.SetBool("isPlayerAttacking", true);
            agent.speed = 5f;
            isEnemy = true;
            agent.stoppingDistance = 1f;
            if (Physics.Raycast(ray.origin, ray.direction, out infoHit))
            {
                agent.stoppingDistance = 2f;
                agent.destination = infoHit.point;
            }
        }
        else
        {
            anim.SetBool("isPlayerAttacking", false);
            anim.SetBool("Attack", false);
            agent.stoppingDistance = 0f;
            isEnemy = false;
            actualEnemy = null;

            if (Time.time < InitialTouch + 0.5f)
            {
                agent.speed = 6f;
                if (Physics.Raycast(ray.origin, ray.direction, out infoHit))
                {
                    agent.destination = infoHit.point;
                }
            }
            else
            {
                agent.speed = 3f;
                if (Physics.Raycast(ray.origin, ray.direction, out infoHit))
                {
                    agent.destination = infoHit.point;
                }
            }
            InitialTouch = Time.time;
        }
    }

    public void StopCharacter()
    {
        anim.SetBool("isPlayerAttacking", false);
        anim.SetBool("Attack", false);
        isEnemy = false;
        agent.destination = this.gameObject.transform.position;
        agent.speed = 0f;

    }

    private void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.tag == "EnemyWeapon")
        {
            var EnemyStats = col.gameObject.GetComponent<EnemyBehaviourContainer>().enemy;
            characterDefinition.TakeDamage(EnemyStats.enemyDefinition.currentDamage);
        }
    }

    void OnEnable()
    {
        EventManager.StartListening(EventManager.ChestOpened, StopCharacter);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventManager.ChestOpened, StopCharacter);

    }

}
