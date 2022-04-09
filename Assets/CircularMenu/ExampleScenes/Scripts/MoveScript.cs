using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveScript : MonoBehaviour 
{
    public List<Transform> steps;
    public int actualStep;

    public float vitesse = 5;

    void Awake()
    {
        actualStep = 0;
    }

    void Update()
    {
        Move();
    }

    private void NextStep()
    {
        actualStep++;

        if (actualStep >= steps.Count)
            actualStep = 0;
    }

    private void Move()
    {
        Vector3 newPosition = Vector3.Lerp(transform.position, steps[actualStep].position, Time.deltaTime * vitesse);
        newPosition.y = 1;
        transform.position = newPosition;

        if (Vector3.Distance(transform.position, steps[actualStep].position) < 1.5f)
            NextStep();
    }

}
