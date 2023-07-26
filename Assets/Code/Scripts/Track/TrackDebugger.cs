using FR8.Track;
using UnityEngine;

namespace FR8
{
    public class TrackDebugger : MonoBehaviour
    {
        [SerializeField] private TrackWalker walker;
        
        void Update()
        {
            walker.Walk(transform.position);
            Debug.DrawLine(transform.position, walker.Position);
            Debug.DrawRay(transform.position, walker.Tangent);
        }
    }
}
