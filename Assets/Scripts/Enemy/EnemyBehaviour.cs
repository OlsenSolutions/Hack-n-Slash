using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class EnemyBehaviour : MonoBehaviour
{
    public EnemyStats_SO enemyDefinition;

    private EnemyStats enemyStats;

    private CharacterStats charStats;

    private GameObject player;
    private Animator anim;

    public SpriteRenderer bar;

    public BoxCollider weaponCollider;
    public ParticleSystem DeathParticle;
    public AudioSource[] SwordSounds;

    public AudioSource Death;

    void Awake()
    {

    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
        enemyStats = GetComponent<EnemyStats>();
        setRigidbodyState(true);
        setCollidersState(false);
    }

    void Update()
    {
        if (player != null)
        {
            if (charStats == null)
            {
                FindPlayer();
            }

            if (charStats.characterDefinition.currentHealth <= 0)
            {
                anim.SetBool("PlayerIsNear", false);

            }
            if (enemyDefinition.currentHealth <= 0)
            {
                player.GetComponent<CharacterStats>().GiveExperience(enemyDefinition.experienceAdded);
                player.GetComponent<CharacterBehaviour>().StopCharacter();
                Dead();
            }
        }
        else
            player = GameObject.FindGameObjectWithTag("Player");
    }

    void FindPlayer()
    {
        charStats = player.GetComponent<CharacterStats>();

    }

    void Dead()
    {
        bar.gameObject.SetActive(false);
        EventManager.TriggerEvent(EventManager.EnemyDead);
        DeathParticle.Play();
        anim.enabled = false;
        setRigidbodyState(false);
        setCollidersState(true);
        enemyStats.HealthBar.SetActive(false);
        enemyStats.ManaBar.SetActive(false);
        Death.Play();
        this.gameObject.tag = "Untagged";
        StartCoroutine(DeadDelayStatic());
        SetLayerRecursively(this.gameObject, 10);
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<EnemyAI>().enabled = false;
        GetComponent<EnemyBehaviour>().enabled = false;
    }

    private static void SetLayerRecursively(GameObject go, int layerNumber)
    {
        if (go == null) return;
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }

    IEnumerator DeadDelayStatic()
    {
        yield return new WaitForSeconds(3);
    }

    private void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.tag == "Weapon")
        {
            if (charStats != null)
            {
                enemyStats.TakeDamage(charStats.characterDefinition.currentDamage);
                var random = new System.Random();
                int x = random.Next(0, SwordSounds.Length - 1);
                SwordSounds[x].Play();
            }
            else
            {
                FindPlayer();
            }
        }
    }

    void setRigidbodyState(bool state)
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = state;
        }
        if (state == true)
            GetComponent<Rigidbody>().isKinematic = state;
        else if (state == false)
            GetComponent<Rigidbody>().isKinematic = !state;
    }

    void setCollidersState(bool state)
    {
        Collider[] collidersbodies = GetComponentsInChildren<Collider>();
        foreach (Collider collider in collidersbodies)
        {
            if (collider.transform.tag != "EnemyCollider")
                collider.enabled = state;
        }
        GetComponent<CapsuleCollider>().enabled = !state;
        weaponCollider.enabled = !state;
    }

}
