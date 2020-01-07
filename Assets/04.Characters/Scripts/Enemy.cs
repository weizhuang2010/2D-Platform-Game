using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private int m_HP = 10;
    private Rigidbody2D m_Rigidbody2d;

	private void Awake()
	{
        m_Rigidbody2d = GetComponent<Rigidbody2D>();
    }

	private void OnCollisionEnter2D(Collision2D other)
	{
        if (other.gameObject.tag == "Player")
		{
            Debug.Log("Hit.");
        }
    }

    public void ApplyDamage(int value)
	{
        m_HP -= value;
        Debug.Log(m_HP);
    }
}
