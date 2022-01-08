using SocketIOClient;
using System;
using UnityEngine;

namespace SimpleSocketIOChatSDK
{
    public class SocketIOChatManager : MonoBehaviour
    {
        private static SocketIOChatManager instance;
        public static SocketIOChatManager Instance
        {
            get
            {
                if (!instance)
                    new GameObject("_SocketIOChatManager").AddComponent<SocketIOChatManager>();
                return instance;
            }
        }

        public string serviceAddress = "http://localhost:8215";
        public event Action<SocketIOResponse> onMsg;
        private SocketIO client;

        private void Awake()
        {
            if (instance)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        public async void Connect()
        {
            Disconnect();
            client = new SocketIO(serviceAddress);
            await client.ConnectAsync();
        }

        public async void Disconnect()
        {
            if (client != null && client.Connected)
                await client.DisconnectAsync();
            client = null;
        }

        public async void ValidateUser(SendValidateUser data)
        {
            await client.EmitAsync("validate-user", data);
        }
    }
}
