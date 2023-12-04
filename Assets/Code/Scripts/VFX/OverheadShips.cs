using System.Collections;
using UnityEngine;

namespace FR8.Runtime.VFX
{
    public class OverheadShips : MonoBehaviour
    {
        public float elevation = 500.0f;
        public float speed = 100.0f;
        public float spread = 1000.0f;
        public float transitionTime = 5.0f;

        private Vector3 start;
        private Vector3 end;
        private float length;
        private float distance;
        private bool transitionActive;

        private void Start()
        {
            FindNewTrip();
            StartCoroutine(TransitionIn());
        }

        private void FindNewTrip()
        {
            var r = Random.insideUnitCircle * spread;
            start = new Vector3(r.x, elevation, r.y);
            
            r = Random.insideUnitCircle * spread;
            end = new Vector3(start.x + r.x, elevation, start.z + r.y);

            length = (end - start).magnitude;
            transform.rotation = Quaternion.LookRotation(end - start, Vector3.up);
        }

        private void FixedUpdate()
        {
            distance += speed * Time.deltaTime;
            transform.position = Vector3.LerpUnclamped(start, end, distance / length);
            if (distance > length && transitionActive)
            {
                StartCoroutine(Transition());
            }
        }
        
        private IEnumerator TransitionIn()
        {
            var p = 0.0f;
            while (p < 1.0f)
            {
                transform.localScale = Vector3.one * p;
                p += Time.deltaTime / transitionTime;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }
        
        private IEnumerator TransitionOut()
        {
            var p = 0.0f;
            while (p < 1.0f)
            {
                transform.localScale = Vector3.one * (1.0f - p);
                p += Time.deltaTime / transitionTime;
                yield return null;
            }
            transform.localScale = Vector3.zero;
        }
        
        private IEnumerator Transition()
        {
            transitionActive = true;
            yield return StartCoroutine(TransitionOut());
            FindNewTrip();
            yield return StartCoroutine(TransitionIn());
            distance = 0.0f;
            transitionActive = false;
        }
    }
}
