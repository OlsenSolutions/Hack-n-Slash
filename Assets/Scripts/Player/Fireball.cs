using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public ParticleSystem mainParticle;
    public ParticleSystem WallHit;
    public ParticleSystem EnemyHit;
    public GameObject Light;
    public int Damage;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy" || col.gameObject.tag == "Wall" || col.gameObject.tag == "BoxCollective")
        {
            if (col.gameObject.tag == "Enemy")
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                if(col.gameObject.GetComponent<EnemyStats>() != null)
                col.gameObject.GetComponent<EnemyStats>().TakeDamage(Damage);
                mainParticle.Stop();
                EnemyHit.Play();
                col.gameObject.GetComponent<EnemyAI>().MagicHit = true;
                GetComponent<SphereCollider>().enabled = false;
                StartCoroutine(WaitToDestroy());

            }
            if (col.gameObject.tag == "Wall")
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                mainParticle.Stop();
                WallHit.Play();
                GetComponent<SphereCollider>().enabled = false;
                StartCoroutine(WaitToDestroy());

            }
            if (col.gameObject.tag == "BoxCollective")
            {
                col.gameObject.GetComponent<BoxBehaviour>().MagicHit(this.gameObject);

            }
        }
    }

    IEnumerator WaitToDestroy()
    {
        Light.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }
}
