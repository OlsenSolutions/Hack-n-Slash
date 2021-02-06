using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxBehaviour : MonoBehaviour
{
    public GameObject hpPotion;
    public GameObject manaPotion;
    public GameObject expPotion;
    public MeshRenderer boxMesh;
    public GameObject ps;
    private GameObject player;
    private Animator anim;

    public ParticleSystem animBox;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Weapon" || col.gameObject.tag == "Magic")
        {
            InstantiatePotion();
            anim.SetBool("DestroyBox", true);
            ps.SetActive(true);
            ps.GetComponent<ParticleSystem>().Play(true);
            animBox.gameObject.SetActive(false);
            DeactivateBox();

        }
    }

    private void DeactivateBox()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<BoxCollider>().isTrigger = true;
        boxMesh.enabled = false;
        EventManager.TriggerEvent(EventManager.ChestOpened);
    }

    private void InstantiatePotion()
    {
        float x = UnityEngine.Random.Range(0f, 100f);
        if (x > 60f)
        {
            GameObject potion = Instantiate(hpPotion, this.gameObject.transform.position, this.gameObject.transform.rotation) as GameObject;


        }
        else if (x <= 60f && x > 20f)
        {
            GameObject potion = Instantiate(manaPotion, this.gameObject.transform.position, this.gameObject.transform.rotation) as GameObject;

        }
        else
        {
            GameObject potion = Instantiate(expPotion, this.gameObject.transform.position, this.gameObject.transform.rotation) as GameObject;
        }

    }

}
