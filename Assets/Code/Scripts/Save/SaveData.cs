using System;

namespace FR8Runtime.Save
{
    [Serializable]
    public class SaveData
    {
        public PlayerSaveData player;
        public TrainSaveData[] trains;
    }
}