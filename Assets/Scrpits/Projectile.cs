using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int damage;
    [SerializeField] private float speed;
    private GameManager gameManager;
    private Rigidbody2D rig;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        rig = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        rig.velocity = speed * transform.up;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameManager.DealDamage(damage);
            Destroy(gameObject);
        }
    }

    public void SetDamage(int x) { damage = x; }
}
