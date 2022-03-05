using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeWall : MonoBehaviour
{
    #region Variables
    [SerializeField]
    Transform startPos;
    [SerializeField]
    Transform endPos;

    [SerializeField]
    float pathProgress;

    [SerializeField]
    float progressMod;
    [SerializeField]
    float sinHeightMod;
    [SerializeField]
    float sinSpeedMod;


    #endregion
    #region Core Functions
    void Update()
    {
        HandleProgress();
        Move();
        AddSinOffset();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(startPos.position, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPos.position, 0.2f);
        Gizmos.DrawLine(startPos.position, endPos.position);
    }
    #endregion
    #region Functions
    void HandleProgress()
    {
        pathProgress += Time.deltaTime * progressMod;
        if (pathProgress > 1 || pathProgress < 0)
        {
            progressMod = -progressMod;
        }
    }
    void Move()
    {
        gameObject.transform.position = Vector3.Lerp(startPos.position, endPos.position, pathProgress);
    }
    void AddSinOffset()
    {
        float sinValue = Mathf.Sin(Time.realtimeSinceStartup * Mathf.PI * sinSpeedMod) * sinHeightMod;
        gameObject.transform.position = new Vector3(transform.position.x, transform.position.y + sinValue, transform.position.z);
    }


    #endregion
}
