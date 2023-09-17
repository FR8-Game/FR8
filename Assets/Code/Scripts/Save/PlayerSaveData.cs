
using System;
using FR8Runtime.Player;
using UnityEngine;

namespace FR8Runtime.Save
{
    [Serializable]
    public class PlayerSaveData
    {
        public float px, py, pz;
        public float rotation;

        public PlayerSaveData() { }

        public PlayerSaveData(PlayerAvatar avatar)
        {
            px = avatar.transform.position.x;
            py = avatar.transform.position.y;
            pz = avatar.transform.position.z;

            rotation = avatar.transform.eulerAngles.y;
        }

        public void Apply(PlayerAvatar avatar)
        {
            avatar.transform.position = new Vector3(px, py, pz);
            avatar.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
    }
}