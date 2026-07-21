using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

/// <summary>
/// C# Script Template for CSSCRIPT Plugin Command
/// 
/// This is a template for creating .csx scripts to run via the CSSCRIPT plugin command.
/// 
/// USAGE:
///   CSSCRIPT template.csx
///   CSSCRIPT template.csx arg1 arg2 arg3
/// 
/// REQUIREMENTS:
///   - Script must have a Program class
///   - Program class must have a static void Main() or static void Main(string[] args) method
///   - Use Console.WriteLine() for output
///   - Use Console.Error.WriteLine() for error messages
///   - Return codes: Main() is void, so return via Environment.Exit() if needed
/// </summary>

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Example 1: Print all arguments
            if (args.Length > 0)
            {
                Console.WriteLine("Arguments received:");
                for (int i = 0; i < args.Length; i++)
                {
                    Console.WriteLine("  args[" + i + "] = " + args[i]);
                }
            }
            else
            {
                Console.WriteLine("No arguments provided.");
            }

            // Example 2: Parse numeric arguments
            if (args.Length >= 2)
            {
                int num1, num2;
                if (int.TryParse(args[0], out num1) && int.TryParse(args[1], out num2))
                {
                    Console.WriteLine(string.Format("\nSum: {0} + {1} = {2}", num1, num2, num1 + num2));
                    Console.WriteLine(string.Format("Product: {0} * {1} = {2}", num1, num2, num1 * num2));
                }
                else
                {
                    Console.Error.WriteLine("Error: First two arguments must be integers");
                }
            }

            // Example 3: String manipulation
            if (args.Length > 0)
            {
                string text = args[0];
                Console.WriteLine("\nString Examples:");
                Console.WriteLine("  Original: " + text);
                Console.WriteLine("  Upper:    " + text.ToUpperInvariant());
                Console.WriteLine("  Lower:    " + text.ToLowerInvariant());
                Console.WriteLine("  Length:   " + text.Length);
            }

            // Example 4: Work with dates
            DateTime today = DateTime.Today;
            Console.WriteLine("\nToday's Date: " + today.ToString("yyyy-MM-dd"));
            Console.WriteLine("Current Time: " + DateTime.Now.ToString("HH:mm:ss"));

            // Example 5: Simple calculation
            double pi = 3.14159265359;
            double radius = 5.0;
            double area = pi * radius * radius;
            Console.WriteLine("\nCircle with radius " + radius + ": Area = " + area.ToString("F2"));

            // Example 6: Check for palindrome
            int n = 121;
            int temp = n;
            int rev = 0;

            while (temp != 0)
            {
                rev = rev * 10 + temp % 10;
                temp /= 10;
            }

            Console.WriteLine("Is " + n + " a palindrome?");
            Console.WriteLine(n == rev ? "Yes" : "No");

            // Example 7: LINQ Queries Example
            string[] names = { "One", "Two", "Three" };
            IEnumerable<string> filteredNames = System.Linq.Enumerable.Where(names, n7 => n7.Length >= 4);

            foreach (string n7 in filteredNames)
            {
                Console.WriteLine(n7);
            }

            // Example 8: Call TCC commands via TccRun (uses reflection helper to avoid compile-time dependency)
            Console.WriteLine("\nTCC PShell get-date: " + CallTccRun("PSHELL /S \"get-date\"").TrimEnd());
            Console.WriteLine("TCC PShell 2026-1960: " + CallTccRun("PSHELL /S \"2026-1960\"").TrimEnd());
            // Uncomment and change path to test with your own .ps1 file:
            // Console.WriteLine("TCC PShell script.ps1: " + CallTccRun("PSHELL \"e:\\utils\\WinVer.ps1\"").TrimEnd());

            // Example 9: Square each number in an array,
            // without using loops.
            // Example input array
            int[] numbers = { 1, 2, 3, 4, 5 };

            // Square each number without explicit loops
            int[] squaredNumbers = numbers
                .Select(n9 => checked(n9 * n9)) // checked to detect overflow
                .ToArray();

            // Display results
            Console.WriteLine("Original: " + string.Join(", ", numbers));
            Console.WriteLine("Squared: " + string.Join(", ", squaredNumbers));

            // Example 10: Lambda Expressions
            // Ref: https://jpsoft.com/forums/threads/c-cli-doesnt-support-lambda-expressions.12538/
            string input = "24 +13+12 +21 +46 +14+10+20+14+   30     +18+ 50";

            var sortedNumbers = input
                .Split('+')
                .Select(int.Parse)
                .OrderBy(n10 => n10)
                .ToArray();

            Console.WriteLine(string.Join("+", sortedNumbers));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
        }
    }

    // Helper: attempt to call CsScriptPlugin.CsScript.TccRun via reflection, fallback to TakeCommand.PluginHost.InvokeCommand
    private static string CallTccRun(string cmd)
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Try to find CsScriptPlugin.CsScript.TccRun first
            foreach (var asm in assemblies)
            {
                var t = asm.GetType("CsScriptPlugin.CsScript") ?? asm.GetType("CsScript.CsScript");
                if (t != null)
                {
                    var m = t.GetMethod("TccRun", BindingFlags.Public | BindingFlags.Static);
                    if (m != null)
                    {
                        var res = m.Invoke(null, new object[] { cmd });
                        return res != null ? res.ToString() : string.Empty;
                    }
                }
            }

            // Fallback: Try TakeCommand.PluginHost.InvokeCommand
            foreach (var asm in assemblies)
            {
                var host = asm.GetType("TakeCommand.PluginHost");
                if (host != null)
                {
                    var m = host.GetMethod("InvokeCommand", BindingFlags.Public | BindingFlags.Static);
                    if (m != null)
                    {
                        var res = m.Invoke(null, new object[] { cmd });
                        return res != null ? res.ToString() : string.Empty;
                    }
                }
            }

            return "[TccRun unavailable]";
        }
        catch (Exception ex)
        {
            return "[TccRun error: " + ex.Message + "]";
        }
    }
}

