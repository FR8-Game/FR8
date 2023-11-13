using FR8.Runtime.Player;

namespace FR8.Runtime
{
    public interface IVitalityBooster : IBehaviour
    {
        bool CanUse(PlayerAvatar avatar);
        void Bind(PlayerAvatar avatar);
        void Unbind(PlayerAvatar avatar);
    }
}