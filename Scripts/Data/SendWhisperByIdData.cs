namespace SimpleSocketIOChatSDK
{
    [System.Serializable]
    public struct SendWhisperByIdData
    {
        public string targetUserId;
        public string msg;
    }
}
