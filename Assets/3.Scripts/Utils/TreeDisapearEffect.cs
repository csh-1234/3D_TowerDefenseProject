using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TreeDisapearEffect : MonoBehaviour
{
    public ParticleSystem effect;
    public float checkRadius = .5f;

    private void Awake()
    {
        effect = Resources.Load<ParticleSystem>("Effects/TreeEffect");
    }
}