namespace SimpleSocketIOChatSDK
{
    [System.Serializable]
    public struct RecvWhisperData
    {
        public string user_id;
        public string name;
        public string msg;
    }
}
