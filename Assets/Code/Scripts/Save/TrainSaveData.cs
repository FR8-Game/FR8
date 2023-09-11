using System;
using FR8Runtime.Train;
using UnityEngine;

namespace FR8Runtime.Save
{
    [Serializable]
    public class TrainSaveData
    {
        public string saveTypeReference;
        public float px, py, pz;
        public float rx, ry, rz;

        public TrainSaveData() { }

        public TrainSaveData(TrainCarriage carriage)
        {
            saveTypeReference = carriage.saveTypeReference;

            var transform = carriage.transform;

            px = transform.position.x;
            py = transform.position.y;
            pz = transform.position.z;

            rx = transform.eulerAngles.x;
            ry = transform.eulerAngles.y;
            rz = transform.eulerAngles.z;
        }

        public void Apply(TrainCarriage carriage)
        {
            var transform = carriage.transform;

            transform.position = new Vector3(px, py, pz);
            transform.rotation = Quaternion.Euler(rx, ry, rz);
            
            carriage.FindClosestSegment();
        }
    }
}