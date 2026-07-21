// DotNetPluginSDK.cs - .NET Plugin SDK for TCC
// Copyright (c) 2005-2026 Rex C. Conn  All rights reserved
//
// To create a .NET plugin for TCC:
//   1. Create a .NET Framework 4.8 (or .NET Standard 2.0) class library project.
//   2. Include this file (or reference the compiled TakeCommand.Plugin.SDK.dll).
//   3. Create a public class that implements ITCCPlugin.
//   4. Build your DLL and place it in the TCC PlugIns directory.
//
// Function naming conventions in the Functions list:
//   "MYCOMMAND"   - internal command  (method: public int MYCOMMAND(StringBuilder args))
//   "_MYVAR"      - internal variable (method: public int _MYVAR(StringBuilder args))
//   "@MYFUNC"     - variable function (method: public int f_MYFUNC(StringBuilder args))
//   "*MYKEYS"     - keystroke handler (not supported for .NET plugins)
//
// Method signatures:
//   - Command/variable/function methods receive a StringBuilder containing the arguments.
//     Write results back into the StringBuilder for variables and functions.
//   - Return 0 for success.
//   - Return 0xFEDCBA98 (unchecked) to indicate the plugin did not handle the call,
//     allowing TCC to continue searching for a matching internal or external command.
//
// Example:
//
//   public class MyPlugin : ITCCPlugin
//   {
//       private static readonly TCCPluginInfo _info = new TCCPluginInfo
//       {
//           Name        = "MyPlugin",
//           Author      = "Your Name",
//           Email       = "you@example.com",
//           WWW         = "https://example.com",
//           Description = "A sample TCC plugin written in C#",
//           Functions   = "HELLO,_MYPLUGINVER,@REVERSE",
//           Major       = 1,
//           Minor       = 0,
//           Build       = 1
//       };
//
//       public TCCPluginInfo GetPluginInfo() => _info;
//       public bool Initialize()            => true;
//       public bool Shutdown(bool end)       => true;
//
//       public int HELLO(StringBuilder args)
//       {
//           // A simple command
//           Console.WriteLine("Hello from .NET!");
//           return 0;
//       }
//
//       public int _MYPLUGINVER(StringBuilder args)
//       {
//           args.Clear();
//           args.Append("1.0.1");
//           return 0;
//       }
//
//       public int f_REVERSE(StringBuilder args)
//       {
//           char[] chars = args.ToString().ToCharArray();
//           Array.Reverse(chars);
//           args.Clear();
//           args.Append(chars);
//           return 0;
//       }
//   }using System;
using System.Text;namespace TakeCommand.Plugin
{
    /// <summary>
    /// Metadata about a TCC plugin.
    /// </summary>
    public class TCCPluginInfo
    {
        /// <summary>Short name of the plugin (shown as the DLL name in TCC).</summary>
        public string Name { get; set; }    /// <summary>Author's name.</summary>
    public string Author { get; set; }

    /// <summary>Author's email address.</summary>
    public string Email { get; set; }

    /// <summary>Author's web page.</summary>
    public string WWW { get; set; }

    /// <summary>Brief description of the plugin.</summary>
    public string Description { get; set; }

    /// <summary>
    /// Comma-delimited list of functions/commands/variables the plugin provides.
    /// Prefix '_' for internal variables, '@' for variable functions, '*' for
    /// keystroke handlers, or no prefix for commands.
    /// </summary>
    public string Functions { get; set; }

    /// <summary>Major version number.</summary>
    public int Major { get; set; }

    /// <summary>Minor version number.</summary>
    public int Minor { get; set; }

    /// <summary>Build number.</summary>
    public int Build { get; set; }
}

/// <summary>
/// Interface that all .NET TCC plugins must implement.
/// </summary>
public interface ITCCPlugin
{
    /// <summary>
    /// Return metadata about this plugin.  Called once after loading.
    /// </summary>
    TCCPluginInfo GetPluginInfo();

    /// <summary>
    /// Called by TCC after the assembly is loaded.
    /// Return true on success, false on failure.
    /// </summary>
    bool Initialize();

    /// <summary>
    /// Called by TCC when the plugin is being unloaded.
    /// <paramref name="endProcess"/> is true when the entire command processor
    /// is shutting down, false when only this plugin is being unloaded.
    /// Return true on success.
    /// </summary>
    bool Shutdown(bool endProcess);
}

}