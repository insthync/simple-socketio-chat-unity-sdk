using System.Collections.Generic;

namespace SimpleSocketIOChatSDK
{
    [System.Serializable]
    public struct RecvGroupUserListData
    {
        public string groupId;
        public List<EntryGroupUserData> list;
    }
}
