using SocketIOClient;
using System;
using System.Collections.Generic;
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
                if (!instance)
                    new GameObject("_SocketIOChatManager").AddComponent<SocketIOChatManager>();
                return instance;
            }
        }

        public string serviceAddress = "http://localhost:8215";
        public string serviceSecretKey = "secret";
        public event Action<EntryUserData> onAddUser;
        public event Action<RecvLocalData> onRecvLocal;
        public event Action<RecvGlobalData> onRecvGlobal;
        public event Action<RecvWhisperData> onRecvWhisper;
        public event Action<RecvGroupData> onRecvGroup;
        public event Action<RecvCreateGroupData> onRecvCreateGroup;
        public event Action<RecvUpdateGroupData> onRecvUpdateGroup;
        public event Action<RecvGroupInvitationListData> onRecvGroupInvitationList;
        public event Action<RecvGroupListData> onRecvGroupList;
        public event Action<RecvGroupJoinData> onRecvGroupJoin;
        public event Action<RecvGroupLeaveData> onRecvGroupLeave;
        public Dictionary<string, EntryUserData> Users { get; private set; } = new Dictionary<string, EntryUserData>();
        public Dictionary<string, EntryGroupData> Groups { get; private set; } = new Dictionary<string, EntryGroupData>();
        public Dictionary<string, EntryGroupData> GroupInvitations { get; private set; } = new Dictionary<string, EntryGroupData>();
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

        private async void OnDestroy()
        {
            await Disconnect();
        }

        public async Task Connect()
        {
            await Disconnect();
            client = new SocketIO(serviceAddress);
            client.On("local", OnLocal);
            client.On("global", OnGlobal);
            client.On("whisper", OnWhisper);
            client.On("group", OnGroup);
            client.On("create-group", OnCreateGroup);
            client.On("update-group", OnUpdateGroup);
            client.On("group-invitation-list", OnGroupInvitationList);
            client.On("group-list", OnGroupList);
            client.On("group-join", OnGroupJoin);
            client.On("group-leave", OnGroupLeave);
            Users.Clear();
            Groups.Clear();
            GroupInvitations.Clear();
            await client.ConnectAsync();
        }

        public async Task Disconnect()
        {
            if (client != null && client.Connected)
                await client.DisconnectAsync();
            client = null;
        }

        public async Task AddUser(string user_id, string name)
        {
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add(nameof(user_id), user_id);
            form.Add(nameof(name), name);
            RestClient.Result<EntryUserData> result = await RestClient.Post<Dictionary<string, string>, EntryUserData>(RestClient.GetUrl(serviceAddress, "/add-user"), form, serviceSecretKey);
            if (result.IsNetworkError || result.IsHttpError)
                return;
            Users[result.Content.user_id] = result.Content;
            onAddUser.Invoke(result.Content);
        }

        private void OnLocal(SocketIOResponse resp)
        {
            RecvLocalData data = resp.GetValue<RecvLocalData>();
            onRecvLocal.Invoke(data);
        }

        private void OnGlobal(SocketIOResponse resp)
        {
            RecvGlobalData data = resp.GetValue<RecvGlobalData>();
            onRecvGlobal.Invoke(data);
        }

        private void OnWhisper(SocketIOResponse resp)
        {
            RecvWhisperData data = resp.GetValue<RecvWhisperData>();
            onRecvWhisper.Invoke(data);
        }

        private void OnGroup(SocketIOResponse resp)
        {
            RecvGroupData data = resp.GetValue<RecvGroupData>();
            onRecvGroup.Invoke(data);
        }

        private void OnCreateGroup(SocketIOResponse resp)
        {
            RecvCreateGroupData data = resp.GetValue<RecvCreateGroupData>();
            Groups[data.group_id] = new EntryGroupData()
            {
                groupId = data.group_id,
                title = data.title,
                iconUrl = data.icon_url,
            };
            onRecvCreateGroup.Invoke(data);
        }

        private void OnUpdateGroup(SocketIOResponse resp)
        {
            RecvUpdateGroupData data = resp.GetValue<RecvUpdateGroupData>();
            Groups[data.group_id] = new EntryGroupData()
            {
                groupId = data.group_id,
                title = data.title,
                iconUrl = data.icon_url,
            };
            onRecvUpdateGroup.Invoke(data);
        }

        private void OnGroupInvitationList(SocketIOResponse resp)
        {
            RecvGroupInvitationListData data = resp.GetValue<RecvGroupInvitationListData>();
            GroupInvitations.Clear();
            foreach (var entry in data.list)
            {
                GroupInvitations.Add(entry.groupId, new EntryGroupData()
                {
                    groupId = entry.groupId,
                    title = entry.title,
                    iconUrl = entry.iconUrl,
                });
            }
            onRecvGroupInvitationList.Invoke(data);
        }

        private void OnGroupList(SocketIOResponse resp)
        {
            RecvGroupListData data = resp.GetValue<RecvGroupListData>();
            Groups.Clear();
            foreach (var entry in data.list)
            {
                Groups.Add(entry.groupId, new EntryGroupData()
                {
                    groupId = entry.groupId,
                    title = entry.title,
                    iconUrl = entry.iconUrl,
                });
            }
            onRecvGroupList.Invoke(data);
        }

        private void OnGroupJoin(SocketIOResponse resp)
        {
            RecvGroupJoinData data = resp.GetValue<RecvGroupJoinData>();
            onRecvGroupJoin.Invoke(data);
        }

        private void OnGroupLeave(SocketIOResponse resp)
        {
            RecvGroupLeaveData data = resp.GetValue<RecvGroupLeaveData>();
            onRecvGroupLeave.Invoke(data);
        }

        public async Task SendValidateUser(SendValidateUser data)
        {
            await client.EmitAsync("validate-user", data);
        }

        public async Task SendLocal(SendLocalData data)
        {
            await client.EmitAsync("local", data);
        }

        public async Task SendGlobal(SendGlobalData data)
        {
            await client.EmitAsync("global", data);
        }

        public async Task SendWhisper(SendWhisperData data)
        {
            await client.EmitAsync("whisper", data);
        }

        public async Task SendGroup(SendGroupData data)
        {
            await client.EmitAsync("group", data);
        }

        public async Task SendGroupList()
        {
            await client.EmitAsync("group-list");
        }

        public async Task SendCreateGroup(SendCreateGroupData data)
        {
            await client.EmitAsync("create-group", data);
        }

        public async Task SendUpdateGroup(SendUpdateGroupData data)
        {
            await client.EmitAsync("update-group", data);
        }

        public async Task SendGroupInvitationList()
        {
            await client.EmitAsync("group-invitation-list");
        }

        public async Task SendGroupInvite(SendGroupInviteData data)
        {
            await client.EmitAsync("group-invite", data);
        }

        public async Task SendGroupInviteAccept(SendGroupInviteAcceptData data)
        {
            await client.EmitAsync("group-invite-accept", data);
        }

        public async Task SendGroupInviteDecline(SendGroupInviteDeclineData data)
        {
            await client.EmitAsync("group-invite-decline", data);
        }

        public async Task SendLeaveGroup(SendLeaveGroupData data)
        {
            await client.EmitAsync("leave-group", data);
        }

        public async Task SendKickUser(SendKickUserData data)
        {
            await client.EmitAsync("kick-user", data);
        }
    }
}
