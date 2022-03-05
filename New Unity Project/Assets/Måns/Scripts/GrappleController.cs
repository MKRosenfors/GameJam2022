using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cameraTransform;

    [Header("Grapple")]
    [SerializeField] KeyCode grappleKey;
    [SerializeField] KeyCode grappleReelKey;
    [SerializeField] GameObject grapplePrefab;
    [SerializeField] float grappleThrowMod;
    [SerializeField] float grappleLifetime;
    [SerializeField] float grappleSpeed;
    [SerializeField] float minGrappleDistance;
    [SerializeField] float maxGrappleDistance;
    GameObject currentGrapple;

    void Update()
    {
        DestroyGrappleOnDistance();
        Grapple();
        CheckInput();
    }
    void DestroyGrappleOnDistance()
    {
        if (currentGrapple == null) return;

        if ((currentGrapple.transform.position - transform.position).magnitude > maxGrappleDistance)
        {
            DestroyGrapple();
        }
    }
    void CheckInput()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            ShootGrapple();
        }
        if (Input.GetKeyUp(grappleKey))
        {
            DestroyGrapple();
        }
    }
    void Grapple()
    {
        if (currentGrapple == null) return;
        if (currentGrapple.GetComponent<Grapple>().isGrappled == false) return;
        if ((currentGrapple.transform.position - transform.position).magnitude < minGrappleDistance) return;

        if (Input.GetKey(grappleReelKey))
        {
            GetComponent<Rigidbody>().AddForce((
                currentGrapple.transform.position - transform.position)
                * grappleSpeed);
        }
    }
    void ShootGrapple()
    {
        currentGrapple = Instantiate(grapplePrefab, cameraTransform.position, cameraTransform.rotation);
        Grapple grapple = currentGrapple.GetComponent<Grapple>();
        grapple.SetVelocity(grappleThrowMod);
        grapple.SetLifetime(grappleLifetime);
        grapple.SetSource(gameObject);
    }
    void DestroyGrapple()
    {
        Destroy(currentGrapple);
    }
}
