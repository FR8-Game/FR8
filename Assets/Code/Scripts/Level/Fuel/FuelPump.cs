using System;
using FR8.Sockets;
using UnityEngine;

namespace FR8.Level.Fuel
{
    [SelectionBase, DisallowMultipleComponent]
    public class FuelPump : MonoBehaviour
    {
        private SocketManager socketManager;
        private FuelPumpHandle handle;

        private void Awake()
        {
            handle = GetComponentInChildren<FuelPumpHandle>();
            socketManager = GetComponentInChildren<SocketManager>();
        }

        private void Start()
        {
            socketManager.Bind(handle);
        }
    }
}