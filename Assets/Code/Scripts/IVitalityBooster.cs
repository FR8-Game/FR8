using FR8Runtime.Player;

namespace FR8Runtime
{
    public interface IVitalityBooster : IBehaviour
    {
        bool CanUse(PlayerAvatar avatar);
        void Bind(PlayerAvatar avatar);
        void Unbind(PlayerAvatar avatar);
    }
}