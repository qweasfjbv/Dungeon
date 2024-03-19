using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PathTest : MonoBehaviour
{

    public List<Vector2> path;
    
    [SerializeField]
    public float speed;
    private int currentPointIndex = 0;
    void Update()
    {
        GetComponent<Animator>().SetBool("Walk", true);

        if (path == null || path.Count == 0) return; 

        Vector2 currentTarget = path[currentPointIndex];

        var step = speed * new Vector3(currentTarget.x-transform.position.x, currentTarget.y - transform.position.y, 0).normalized;
        GetComponent<Rigidbody2D>().MovePosition(transform.position + new Vector3(step.x, step.y, 0));


        GetComponent<Animator>().SetFloat("X", step.x);
        GetComponent<Animator>().SetFloat("Y", step.y);

        if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
        {
            currentPointIndex++;
            if (currentPointIndex >= path.Count)
            {
                // µµÂø
                this.enabled = false;
            }
        }
    }


    public void SetPath(List<Vector2> newPath)
    {
        path = newPath;
        currentPointIndex = 0;
    }
}