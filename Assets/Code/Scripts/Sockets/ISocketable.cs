
namespace FR8Runtime.Sockets
{
    public interface ISocketable
    {
        bool CanBind();
        ISocketable Bind(SocketManager manager);
        ISocketable Unbind();
        
        string SocketType { get; }
    }
}