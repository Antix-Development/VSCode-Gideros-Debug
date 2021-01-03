namespace GiderosPlayerRemote
{
    static class GiderosMessageType
    {
        // ui -> player
        public const byte CreateFolder = 0;
        public const byte File = 1;
        public const byte Play = 2;
        public const byte Stop = 3;
        public const byte SendFileList = 7;
        public const byte SetProjectName = 8;
        public const byte DeleteFile = 9;
        public const byte SendProjectProperties = 11;

        // player -> ui
        public const byte Output = 4;
        public const byte FileList = 6;
    }
}
