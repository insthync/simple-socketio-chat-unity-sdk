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
            client.On("local", OnLocal);
            client.On("global", OnGlobal);
            client.On("whisper", OnWhisper);
            client.On("group", OnGroup);
            client.On("create-group", OnCreateGroup);
            client.On("update-group", OnUpdateGroup);
            await client.ConnectAsync();
        }

        public async void Disconnect()
        {
            if (client != null && client.Connected)
                await client.DisconnectAsync();
            client = null;
        }

        private void OnLocal(SocketIOResponse resp)
        {
            RecvLocalData data = resp.GetValue<RecvLocalData>();
        }

        private void OnGlobal(SocketIOResponse resp)
        {
            RecvGlobalData data = resp.GetValue<RecvGlobalData>();
        }

        private void OnWhisper(SocketIOResponse resp)
        {
            RecvWhisperData data = resp.GetValue<RecvWhisperData>();
        }

        private void OnGroup(SocketIOResponse resp)
        {
            RecvGroupData data = resp.GetValue<RecvGroupData>();
        }

        private void OnCreateGroup(SocketIOResponse resp)
        {
            RecvCreateGroupData data = resp.GetValue<RecvCreateGroupData>();
        }

        private void OnUpdateGroup(SocketIOResponse resp)
        {
            RecvUpdateGroupData data = resp.GetValue<RecvUpdateGroupData>();
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

        public async void SendGroupInviteAccept(SendGroupInviteAcceptData data)
        {
            await client.EmitAsync("group-invite-accept", data);
        }

        public async void SendGroupInviteDecline(SendGroupInviteDeclineData data)
        {
            await client.EmitAsync("group-invite-decline", data);
        }

        public async void SendLeaveGroup(SendLeaveGroupData data)
        {
            await client.EmitAsync("leave-group", data);
        }

        public async void SendKickUser(SendKickUserData data)
        {
            await client.EmitAsync("kick-user", data);
        }
    }
}
