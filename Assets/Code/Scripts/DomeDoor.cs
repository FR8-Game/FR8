using System;
using FR8.Runtime.Player;
using FR8.Runtime.Train;
using UnityEngine;

namespace FR8.Runtime
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class DomeDoor : MonoBehaviour
    {
        public const int TrainLayer = 10;
        
        public Bounds bounds;
        public Animator target;

        private void OnValidate()
        {
            if (!target) target = GetComponentInChildren<Animator>();
        }

        private void FixedUpdate()
        {
            var query = Physics.OverlapBox
            (
                transform.TransformPoint(bounds.center),
                bounds.extents,
                transform.rotation
            );

            var open = false;
            foreach (var e in query)
            {
                if (!e.GetComponentInParent<TrainCarriage>() && !e.GetComponentInParent<PlayerAvatar>()) continue;
                
                open = true;
                break;
            }
            
            target.SetBool("open", open);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.TransformPoint(bounds.center), bounds.size);
        }
    }
}