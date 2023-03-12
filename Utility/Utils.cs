namespace WooperUtility.Utility
{
    public static class Utils
    {
        public static string LoadJson(string filename)
        {
            return System.IO.File.ReadAllText(filename);
        }
    }
}
