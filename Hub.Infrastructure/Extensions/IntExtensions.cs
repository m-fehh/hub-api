namespace Hub.Infrastructure.Extensions
{
    public static class IntExtensions
    {
        public static string ToExcelColumnName(this int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }
    }
}
