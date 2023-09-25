
namespace FR8Runtime.CodeUtility
{
    public static class StringUtility
    {
        public static string Percent(float p)
        {
            if (p > 1.0f) p = 1.0f;
            if (p < 0.0f) p = 0.0f;
            return $"{p * 100.0f,3:N0}%";
        }
    }
}