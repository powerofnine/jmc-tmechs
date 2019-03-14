using System.Collections;
using System.Collections.Generic;
using TMechs;
using UnityEditor;
using UnityEngine;

public class TestPendulum : MonoBehaviour
{
    public Transform parent;
    public float radius = 10F;

    private CharacterController controller;

    private Vector3 velocity;

    private void Awake()
    {
        if (!parent)
        {
            Debug.LogError("Pendulum has no parent");
            Destroy(gameObject);
        }

        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        velocity.y -= 9.9F * Time.deltaTime;

        Vector3 tensionDir = (parent.position - transform.position).normalized;
        Vector3 sideDir = (Quaternion.Euler(0F, 90F, 0F) * tensionDir).Remove(Utility.Axis.Y);
        sideDir.Normalize();

        float incline = Vector3.Angle(transform.position - parent.position, Vector3.down);

        float tensionForce = 9.81F * Mathf.Cos(incline * Mathf.Deg2Rad);
        float centripetalForce = Mathf.Pow(velocity.magnitude, 2) / radius;
        tensionForce += centripetalForce;

        velocity += tensionDir * tensionForce * Time.deltaTime;

        transform.position = ClampPosition(transform.position + velocity * Time.deltaTime);
    }

    private Vector3 ClampPosition(Vector3 newPos)
    {
        return parent.position + radius * Vector3.Normalize(newPos - parent.position);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 src = parent ? parent.position : transform.position;
        Gizmos.DrawWireSphere(src, radius);
    }
}