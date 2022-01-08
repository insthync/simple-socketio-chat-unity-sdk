using System.Collections.Generic;

namespace SimpleSocketIOChatSDK
{
    [System.Serializable]
    public struct RecvGroupInvitationListData
    {
        public List<EntryUserGroupInvitationData> list;
    }
}
