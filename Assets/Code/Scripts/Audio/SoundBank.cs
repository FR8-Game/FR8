
namespace FR8.Runtime.Audio
{
    public class SoundBank
    {
        public string name;

        public SoundBank(string name)
        {
            this.name = name;
        }

        public string Sound(string name) => $"event:/{this.name}/{name}";
    }
}