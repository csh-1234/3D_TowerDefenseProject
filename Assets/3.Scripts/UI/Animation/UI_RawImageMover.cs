using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RawImageMover : MonoBehaviour
{

    [SerializeField]private RawImage _img;
    public float _x, _y;

    private void Start()
    {
        _x = 0.2f;
        _y = 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        _img.uvRect = new Rect(_img.uvRect.position - new Vector2(_x, _y) * Time.deltaTime, _img.uvRect.size);
    }
}
