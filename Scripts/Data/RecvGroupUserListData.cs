using System.Collections.Generic;

namespace SimpleSocketIOChatSDK
{
    [System.Serializable]
    public struct RecvGroupUserListData
    {
        public List<EntryGroupUserData> list;
    }
}
