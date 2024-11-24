namespace Hub.Infrastructure.Extensions
{
    public static class BoolExtensions
    {
        public static string CheckBool(this bool checkbox)
        {
            return checkbox ? Engine.Get("Yes") : Engine.Get("No");
        }
    }
}
