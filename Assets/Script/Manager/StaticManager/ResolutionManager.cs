namespace Script.Manager.StaticManager
{
    public static class ResolutionManager
    {
        public static void Initialize()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        int rWidth = Display.main.systemWidth;
        int rHeight = Display.main.systemHeight - 49;
        string str = WindowNativeManager.SetWindowFrame(0, 0, rWidth , rHeight);
#endif
        }
    }
}