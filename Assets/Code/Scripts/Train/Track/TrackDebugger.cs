using UnityEngine;

namespace FR8.Train.Track
{
    
    public class TrackDebugger : MonoBehaviour
    {
        [SerializeField] private TrackWalker walker;
        
        private void OnDrawGizmos()
        {
            walker.Walk(transform.position);
            Debug.DrawLine(transform.position, walker.Position);
            Debug.DrawRay(transform.position, walker.Tangent * 5.0f);
            Debug.DrawRay(walker.Position, walker.Tangent * 5.0f);
        }
    }
}
