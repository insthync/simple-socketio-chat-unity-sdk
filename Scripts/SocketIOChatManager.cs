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

        public async void SendValidateUser(SendValidateUser data)
        {
            await client.EmitAsync("validate-user", data);
        }

        public async void SendLocal(SendLocalData data)
        {
            await client.EmitAsync("local", data);
        }

        public async void SendGlobal(SendGlobalData data)
        {
            await client.EmitAsync("global", data);
        }

        public async void SendWhisper(SendWhisperData data)
        {
            await client.EmitAsync("whisper", data);
        }

        public async void SendGroup(SendGroupData data)
        {
            await client.EmitAsync("group", data);
        }

        public async void SendCreateGroup(SendCreateGroupData data)
        {
            await client.EmitAsync("create-group", data);
        }

        public async void SendUpdateGroup(SendUpdateGroupData data)
        {
            await client.EmitAsync("update-group", data);
        }

        public async void SendInvitationList()
        {
            await client.EmitAsync("group-invitation-list");
        }

        public async void SendGroupInvite(SendGroupInviteData data)
        {
            await client.EmitAsync("group-invite", data);
        }
    }
}
