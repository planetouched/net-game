namespace Preloader
{
    public static class AppStart
    {
        public static bool isInit { get; set; }

        public static void Start()
        {
            isInit = true;
        }
    }
}