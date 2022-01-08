namespace SimpleSocketIOChatSDK
{
    [System.Serializable]
    public struct SendWhisperData
    {
        public string user_id;
        public string target_name;
        public string name;
        public string msg;
    }
}
