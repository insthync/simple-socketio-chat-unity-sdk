namespace SimpleSocketIOChatSDK
{
    [System.Serializable]
    public struct SendUpdateGroupData
    {
        public string user_id;
        public string group_id;
        public string title;
        public string icon_url;
    }
}
