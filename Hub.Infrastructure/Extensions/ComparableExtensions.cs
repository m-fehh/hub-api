namespace Hub.Infrastructure.Extensions
{
    public static class ComparableExtensions
    {
        public static bool IsInRange<T>(this T value, T initial, T final) where T : IComparable
        {
            return value.CompareTo(initial) >= 0 && value.CompareTo(final) <= 0;
        }
    }
}
