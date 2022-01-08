namespace SimpleSocketIOChatSDK
{
    [System.Serializable]
    public struct SendGroupData
    {
        public string user_id;
        public string group_id;
        public string name;
        public string msg;
    }
}
