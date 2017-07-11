using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class AnimationController : MonoBehaviour
    {
        Animator animator;

        public void Update()
        {
            animator = gameObject.GetComponent<Animator>();
            animator.SetBool("is_underwater", true);
        }
    }
}
