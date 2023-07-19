using System;
using UnityEngine;

namespace TerrainHandles.Handles
{
    [SelectionBase]
    public abstract class TerrainHandle : MonoBehaviour
    {
        [SerializeField] private OperationType operationType;

        protected Matrix4x4 localToWorldMatrix;
        protected Matrix4x4 worldToLocalMatrix;
        
        protected Vector3 Position => localToWorldMatrix.GetColumn(3);
        protected Vector3 Up => localToWorldMatrix.GetColumn(1);
        
        private Vector3 MatrixByPoint(Matrix4x4 mat, Vector3 p, float w) => mat * new Vector4(p.x, p.y, p.z, w);
        protected Vector3 LocalToWorld(Vector3 p, float w = 1.0f) => MatrixByPoint(localToWorldMatrix, p, w);
        protected Vector3 WorldToLocal(Vector3 p, float w = 1.0f) => MatrixByPoint(worldToLocalMatrix, p, w);
        
        public virtual void Prepare()
        {
            localToWorldMatrix = transform.localToWorldMatrix;
            worldToLocalMatrix = transform.worldToLocalMatrix;
        }
        
        public abstract float Apply(float w, Vector3 point, TerrainData data);
        public abstract bool IsChunkAffected(Chunk chunk);

        protected float Blend(float a, float b) => Blend(a, b, operationType);
        protected float Blend(float a, float b, OperationType operationType)
        {
            return operationType switch
            {
                OperationType.Add => Mathf.Max(a, b),
                OperationType.Subtract => Mathf.Min(a, -b),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected virtual void OnValidate()
        {
            gameObject.name = GetType().Name;
        }

        public enum OperationType
        {
            Add,
            Subtract,
        }
    }
}