using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    LayerMask grappable;
    GameObject throwSource;
    float grappleLifetime;

    float timer;
    LineRenderer lr;

    public bool isGrappled;

    void Start()
    {
        Debug.Log(grappable);
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        timer = 0f;
    }
    void Update()
    {
        grappable = LayerMask.NameToLayer("Grappable");
        UpdateLineRenderer();
        if (!isGrappled)
        {
            ExpireAfterLifetime();
            transform.LookAt(transform.position + GetComponent<Rigidbody>().velocity);
        }
        
    }
    public void SetVelocity(float velocity)
    {
        GetComponent<Rigidbody>().velocity = transform.forward * velocity;
    }
    public void SetSource(GameObject source)
    {
        throwSource = source;
    }
    public void SetLifetime(float lifetime)
    {
        grappleLifetime = lifetime;
    }
    void UpdateLineRenderer() 
    {
        lr.SetPosition(0, throwSource.transform.position - Vector3.up + throwSource.transform.right/2);
        lr.SetPosition(1, transform.position);
    }
    void ExpireAfterLifetime()
    {
        timer += Time.deltaTime;
        if (timer >= grappleLifetime)
        {
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.layer);
        if (collision.gameObject.layer == grappable)
        {
            Debug.Log("Hit!");
            isGrappled = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
