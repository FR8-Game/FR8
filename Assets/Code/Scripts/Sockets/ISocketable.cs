
using UnityEngine;

namespace FR8.Sockets
{
    public interface ISocketable
    {
        bool CanBind();
        ISocketable Bind(SocketManager manager);
        ISocketable Unbind();
        
        string SocketType { get; }
    }
}