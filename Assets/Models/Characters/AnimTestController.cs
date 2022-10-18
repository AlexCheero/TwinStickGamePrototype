using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimTestController : MonoBehaviour
{
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        _animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
    }
}
