using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeWall : MonoBehaviour
{
    #region Variables
    [SerializeField] Transform startPos;
    [SerializeField] Transform endPos;

    [SerializeField] float pathProgress;
    [SerializeField] float progressMod;


    #endregion
    void Update()
    {
        HandleProgress();
        Move();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(startPos.position, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPos.position, 0.2f);
        Gizmos.DrawLine(startPos.position, endPos.position);
    }
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


    #endregion
}
