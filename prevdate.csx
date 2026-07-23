using System;
using System.Globalization;

public class Program
{
    public static void Main(string[] args)
    {
        if (args == null || args.Length < 1)
        {
            Console.Error.WriteLine("Usage: <program> <date-YYYY-MM-DD>");
            return;
        }

        try
        {
            // Expecting format like: 2026-07-23
            var inputDate = DateTime.ParseExact(
                args[0],
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None);

            var yesterday = inputDate.Date.AddDays(-1);
            Console.WriteLine(yesterday.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
        }
    }
}
