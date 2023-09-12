
using UnityEngine;

namespace FR8Runtime.Audio
{
    public static class Sounds
    {
        public static SoundBank Bank(string bankName) => new(bankName);
    }
}