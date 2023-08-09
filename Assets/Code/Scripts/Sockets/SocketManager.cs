using System.Text.RegularExpressions;
using UnityEngine;

namespace FR8.Sockets
{
    public sealed class SocketManager
    {
        private static readonly Regex SocketRegex = new(@".*\.Socket", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private const float SearchRadius = 0.2f;
        
        private Regex filterRegex;

        private ISocketable currentBinding;

        public Transform SocketTarget { get; private set; }

        public SocketManager(Transform root, string filter)
        {
            SocketTarget = root.DeepFind(SocketRegex);
            filterRegex = new Regex(filter);
        }
        
        public void FixedUpdate()
        {
            if (!SocketTarget) return;

            var list = Physics.OverlapSphere(SocketTarget.position, SearchRadius);
            foreach (var e in list)
            {
                var socketable = e.GetComponentInParent<ISocketable>();
                if (!(Object)socketable) continue;
                if (!socketable.CanBind()) continue;
                if (string.IsNullOrWhiteSpace(socketable.SocketType)) continue;
                if (!filterRegex.IsMatch(socketable.SocketType)) continue;

                currentBinding = socketable.Bind(this);
            }
        }

        public void Bind(ISocketable socketable)
        {
            if (!socketable.CanBind()) return;
            currentBinding = socketable.Bind(this);
        }
        
        public void Unbind()
        {
            if (!(Object)currentBinding) return;
            
            currentBinding = currentBinding.Unbind();
        }
    }
}
