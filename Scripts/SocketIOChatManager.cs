using Cysharp.Threading.Tasks;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityRestClient;

namespace SimpleSocketIOChatSDK
{
    public class SocketIOChatManager : MonoBehaviour
    {
        private static SocketIOChatManager instance;
        public static SocketIOChatManager Instance
        {
            get
            {
                return instance;
            }
        }

        public string serviceAddress = "http://localhost:8215";
        public string serviceSecretKey = "secret";
        public bool autoConnectWhenSend = true;
        public event Action<EntryUserData> onAddUser;
        public event Action<string> onRemoveUser;
        public event Action<RecvLocalData> onRecvLocal;
        public event Action<RecvGlobalData> onRecvGlobal;
        public event Action<RecvWhisperData> onRecvWhisper;
        public event Action<RecvGroupData> onRecvGroup;
        public event Action<RecvCreateGroupData> onRecvCreateGroup;
        public event Action<RecvUpdateGroupData> onRecvUpdateGroup;
        public event Action<RecvGroupInvitationListData> onRecvGroupInvitationList;
        public event Action<RecvGroupUserListData> onRecvGroupUserList;
        public event Action<RecvGroupListData> onRecvGroupList;
        public event Action<RecvGroupJoinData> onRecvGroupJoin;
        public event Action<RecvGroupLeaveData> onRecvGroupLeave;
        public Dictionary<string, EntryUserData> Users { get; private set; } = new Dictionary<string, EntryUserData>();
        public Dictionary<string, EntryGroupData> Groups { get; private set; } = new Dictionary<string, EntryGroupData>();
        public Dictionary<string, EntryGroupData> GroupInvitations { get; private set; } = new Dictionary<string, EntryGroupData>();
        public Dictionary<string, EntryGroupUserData> GroupUsers { get; private set; } = new Dictionary<string, EntryGroupUserData>();
        public Dictionary<string, List<string>> GroupUserIds { get; private set; } = new Dictionary<string, List<string>>();
        private SocketIO client;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[" + nameof(SocketIOChatManager) + "] Connecting to " + serviceAddress);
        }

        private async void OnDestroy()
        {
            await Disconnect();
        }

        public async Task Connect()
        {
            await Disconnect();
            await UniTask.SwitchToMainThread();
            Debug.Log("[" + nameof(SocketIOChatManager) + "] Connecting to " + serviceAddress);
            client = new SocketIO(serviceAddress, new SocketIOOptions()
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            });
            client.On("local", OnLocal);
            client.On("global", OnGlobal);
            client.On("whisper", OnWhisper);
            client.On("group", OnGroup);
            client.On("create-group", OnCreateGroup);
            client.On("update-group", OnUpdateGroup);
            client.On("group-invitation-list", OnGroupInvitationList);
            client.On("group-user-list", OnGroupUserList);
            client.On("group-list", OnGroupList);
            client.On("group-join", OnGroupJoin);
            client.On("group-leave", OnGroupLeave);
            Users.Clear();
            Groups.Clear();
            GroupInvitations.Clear();
            // Always accept SSL
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
            await client.ConnectAsync();
            await UniTask.SwitchToMainThread();
            Debug.Log("[" + nameof(SocketIOChatManager) + "] Connected to " + serviceAddress);
        }

        public async Task Disconnect()
        {
            Debug.Log("[" + nameof(SocketIOChatManager) + "] Disconnecting");
            if (client != null && client.Connected)
                await client.DisconnectAsync();
            await UniTask.SwitchToMainThread();
            Debug.Log("[" + nameof(SocketIOChatManager) + "] Disconnected");
            client = null;
        }

        public async Task AddUser(string userId, string name, string iconUrl)
        {
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add(nameof(userId), userId);
            form.Add(nameof(name), name);
            form.Add(nameof(iconUrl), iconUrl);
            RestClient.Result<EntryUserData> result = await RestClient.Post<Dictionary<string, string>, EntryUserData>(RestClient.GetUrl(serviceAddress, "/add-user"), form, serviceSecretKey);
            if (result.IsNetworkError || result.IsHttpError)
                return;
            Users[result.Content.userId] = result.Content;
            if (onAddUser != null)
                onAddUser.Invoke(result.Content);
        }

        public async Task RemoveUser(string userId)
        {
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add(nameof(userId), userId);
            RestClient.Result result = await RestClient.Post(RestClient.GetUrl(serviceAddress, "/remove-user"), form, serviceSecretKey);
            if (result.IsNetworkError || result.IsHttpError)
                return;
            Users.Remove(userId);
            if (onRemoveUser != null)
                onRemoveUser.Invoke(userId);
        }

        private async void OnLocal(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvLocalData data = resp.GetValue<RecvLocalData>();
            if (onRecvLocal != null)
                onRecvLocal.Invoke(data);
        }

        private async void OnGlobal(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvGlobalData data = resp.GetValue<RecvGlobalData>();
            if (onRecvGlobal != null)
                onRecvGlobal.Invoke(data);
        }

        private async void OnWhisper(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvWhisperData data = resp.GetValue<RecvWhisperData>();
            if (onRecvWhisper != null)
                onRecvWhisper.Invoke(data);
        }

        private async void OnGroup(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvGroupData data = resp.GetValue<RecvGroupData>();
            if (onRecvGroup != null)
                onRecvGroup.Invoke(data);
        }

        private async void OnCreateGroup(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvCreateGroupData data = resp.GetValue<RecvCreateGroupData>();
            Groups[data.groupId] = new EntryGroupData()
            {
                groupId = data.groupId,
                title = data.title,
                iconUrl = data.iconUrl,
            };
            if (onRecvCreateGroup != null)
                onRecvCreateGroup.Invoke(data);
        }

        private async void OnUpdateGroup(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvUpdateGroupData data = resp.GetValue<RecvUpdateGroupData>();
            Groups[data.groupId] = new EntryGroupData()
            {
                groupId = data.groupId,
                title = data.title,
                iconUrl = data.iconUrl,
            };
            if (onRecvUpdateGroup != null)
                onRecvUpdateGroup.Invoke(data);
        }

        private async void OnGroupInvitationList(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvGroupInvitationListData data = resp.GetValue<RecvGroupInvitationListData>();
            GroupInvitations.Clear();
            foreach (var entry in data.list)
            {
                GroupInvitations.Add(entry.groupId, entry);
            }
            if (onRecvGroupInvitationList != null)
                onRecvGroupInvitationList.Invoke(data);
        }

        private async void OnGroupUserList(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvGroupUserListData data = resp.GetValue<RecvGroupUserListData>();
            GroupUsers.Clear();
            GroupUserIds[data.groupId] = new List<string>();
            foreach (var entry in data.list)
            {
                GroupUsers.Add(entry.userId, entry);
                GroupUserIds[data.groupId].Add(entry.userId);
            }
            if (onRecvGroupUserList != null)
                onRecvGroupUserList.Invoke(data);
        }

        private async void OnGroupList(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvGroupListData data = resp.GetValue<RecvGroupListData>();
            Groups.Clear();
            foreach (var entry in data.list)
            {
                Groups.Add(entry.groupId, entry);
            }
            if (onRecvGroupList != null)
                onRecvGroupList.Invoke(data);
        }

        private async void OnGroupJoin(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvGroupJoinData data = resp.GetValue<RecvGroupJoinData>();
            if (onRecvGroupJoin != null)
                onRecvGroupJoin.Invoke(data);
        }

        private async void OnGroupLeave(SocketIOResponse resp)
        {
            await UniTask.SwitchToMainThread();
            RecvGroupLeaveData data = resp.GetValue<RecvGroupLeaveData>();
            if (onRecvGroupLeave != null)
                onRecvGroupLeave.Invoke(data);
        }

        public async Task SendValidateUser(SendValidateUser data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("validate-user", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendLocal(SendLocalData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("local", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendGlobal(SendGlobalData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("global", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendWhisper(SendWhisperData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("whisper", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendWhisperById(SendWhisperByIdData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("whisper-by-id", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendGroup(SendGroupData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("group", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendGroupList()
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("group-list");
            await UniTask.SwitchToMainThread();
        }

        public async Task SendCreateGroup(SendCreateGroupData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("create-group", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendUpdateGroup(SendUpdateGroupData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("update-group", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendGroupInvitationList()
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("group-invitation-list");
            await UniTask.SwitchToMainThread();
        }

        public async Task SendGroupUserList(SendGroupUserListData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("group-user-list", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendGroupInvite(SendGroupInviteData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("group-invite", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendGroupInviteAccept(SendGroupInviteAcceptData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("group-invite-accept", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendGroupInviteDecline(SendGroupInviteDeclineData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("group-invite-decline", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendLeaveGroup(SendLeaveGroupData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("leave-group", data);
            await UniTask.SwitchToMainThread();
        }

        public async Task SendKickUser(SendKickUserData data)
        {
            if (autoConnectWhenSend && (client == null || client.Disconnected))
                await Connect();
            await client.EmitAsync("kick-user", data);
            await UniTask.SwitchToMainThread();
        }
    }
}
