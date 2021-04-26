using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    
    private WaitForSeconds _useAnimWait;
    public WaitForSeconds UseAnimWait => _useAnimWait;
}
