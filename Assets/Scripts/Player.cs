using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float health;

    [SerializeField]
    private float recoverInterval;//恢复时间间隔

    [SerializeField]
    private Transform head;

    private float sumTime;

    private float currentHealth;

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log ("Player Health:"+currentHealth);
    }

    public void Start()
    {
        currentHealth = health;
    }

    public void update()
    {
        sumTime += Time.deltaTime;
        if (sumTime > recoverInterval)
        {
            currentHealth = Mathf.Min(currentHealth + 5, health);
        }
    }

    public Vector3 GetHeadPosition()
    {
        return head.position;
    }
}
