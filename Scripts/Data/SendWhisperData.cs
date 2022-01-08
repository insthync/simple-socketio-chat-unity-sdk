namespace SimpleSocketIOChatSDK
{
    [System.Serializable]
    public struct SendWhisperData
    {
        public string target_name;
        public string name;
        public string msg;
    }
}
