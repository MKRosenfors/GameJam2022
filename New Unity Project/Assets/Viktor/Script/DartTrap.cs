using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartTrap : MonoBehaviour
{
    [SerializeField] int dartsFired;
    [SerializeField] float projectileSpeed;
    [SerializeField] GameObject dartPrefab;
    private Vector3 randomPosition;
    
    private void OnTriggerEnter(Collider player)
    {
        if (player.tag == "Player")
        {
            Debug.Log("Hit Presure Plate");
            for (int i = 0; i < dartsFired; i++)
            {
                randomPosition = new Vector3(transform.position.x + Random.Range(-3f, 3f), transform.position.y + Random.Range(-3f, 3f), transform.position.z + Random.Range(1.1f, 1.6f));
                GameObject dart = Instantiate(dartPrefab, randomPosition, Quaternion.identity);
                dart.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, projectileSpeed * 100f) * Time.deltaTime;
            }

        }

    }
}
