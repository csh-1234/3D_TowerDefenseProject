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

    // OnTriggerEnter 제거 - 이제 PlacementState에서 직접 처리
}