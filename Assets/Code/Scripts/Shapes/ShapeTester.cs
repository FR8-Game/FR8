using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FR8.Runtime.Shapes
{
    public class ShapeTester : MonoBehaviour
    {
        public bool onlyTestClosest;
        public bool testContainsPoint;
        public bool drawShapes = true;

        private StringBuilder handlesText;
        private float handleSize = 1.0f;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var test = false;
            foreach (var s in Selection.gameObjects)
            {
                if (s.gameObject != gameObject) continue;
                test = true;
                break;
            }
            
            handleSize = HandleUtility.GetHandleSize(transform.position);

            if (test)
            {
                Test();
                return;
            }

            DrawSceneText($"[{GetType().Name}] {name} Idle");

            Gizmos.color = ColorReference.Primary;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one * handleSize * 0.1f);
        }

        private void Test()
        {
            var shapes = new List<Shape>(FindObjectsOfType<Shape>());
            handlesText = new StringBuilder();

            var title = $"Results from [{GetType().Name}] {name}";
            handlesText.AppendLine(title);
            handlesText.AppendLine($"- Tested {shapes.Count} Shapes");
            
            if (onlyTestClosest) FilterClosest(shapes);
            if (testContainsPoint) TestContainsPoint(shapes);
            if (drawShapes) DrawShapes(shapes);

            handlesText.Append(new string('-', title.Length));

            DrawSceneText(handlesText.ToString());
        }

        private void DrawSceneText(string text)
        {
            Handles.matrix = Matrix4x4.identity;
            
            var camera = Camera.current;
            var right = camera ? camera.transform.right : transform.right;
            Handles.Label(transform.position + right * handleSize, text);
        }

        private void FilterClosest(List<Shape> shapes)
        {
            var best = shapes[0];
            for (var i = 1; i < shapes.Count; i++)
            {
                var other = shapes[i];

                var score = (other.transform.position - transform.position).sqrMagnitude;
                var bestScore = (best.transform.position - transform.position).sqrMagnitude;

                if (score > bestScore) continue;
                best = other;
            }

            shapes.Clear();
            shapes.Add(best);

            handlesText.AppendLine($"--- Filter Closest ---");
            handlesText.AppendLine($"- Used Closest Shape \"{best.name}\"");
        }

        private void TestContainsPoint(IEnumerable<Shape> shapes)
        {
            var scale = handleSize * 0.1f;
            handlesText.AppendLine($"--- Contains Point Test ---");

            var passed = false;
            foreach (var shape in shapes)
            {
                if (!shape.ContainsPoint(transform.position)) continue;

                passed = true;
                Gizmos.color = ColorReference.Success;
                Gizmos.DrawLine(transform.position, shape.transform.position);
                Gizmos.DrawSphere(shape.transform.position, scale);

                handlesText.AppendLine($"  - Was Found Inside \"{shape.name}\"");
            }

            Gizmos.color = passed ? ColorReference.Success : ColorReference.Failure;
            Gizmos.DrawSphere(transform.position, scale);
        }

        private void DrawShapes(List<Shape> shapes)
        {
            foreach (var e in shapes) e.DrawGizmos();
        }
#endif
    }
}