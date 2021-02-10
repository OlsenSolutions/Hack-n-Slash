using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyBehaviour : MonoBehaviour
{
    private int health = 5;
    private Animator anim;
    private GameObject player;

    private bool isDead;
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Weapon")
        {
            health -= 1;
            anim.SetTrigger("Hit");
        }

    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Magic")
        {
            health -= 5;
        }
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
        isDead = false;
    }
    void Update()
    {
        if (health < 1)
        {
            if (!isDead)
            {
                anim.SetTrigger("Dead");
                gameObject.GetComponent<BoxCollider>().enabled = false;
                if (player != null)
                    player.GetComponent<CharacterBehaviour>().StopCharacter();
                isDead = true;
            }
        }

    }
}
