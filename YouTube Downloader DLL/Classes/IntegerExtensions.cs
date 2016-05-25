namespace YouTube_Downloader_DLL.Classes
{
    public static class IntegerExtensions
    {
        /// <summary>
        /// Returns True if Integer equals any of the given Integers.
        /// </summary>
        public static bool Any(this int i, params int[] ints)
        {
            foreach (int i2 in ints)
                if (i == i2)
                    return true;
            return false;
        }
    }
}
