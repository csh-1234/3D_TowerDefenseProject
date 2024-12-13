using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_StageIndcator : MonoBehaviour
{
    [SerializeField] private float moveDistance = 10f; 
    [SerializeField] private float moveSpeed = 4f;    

    private Vector3 startPosition;
    private float originalY;
    private float time;

    private void Start()
    {
        startPosition = transform.position;
        originalY = startPosition.y;
    }

    private void Update()
    {
        time += Time.deltaTime * moveSpeed;
        float newY = originalY + Mathf.Sin(time) * moveDistance;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
