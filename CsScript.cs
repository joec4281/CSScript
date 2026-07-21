using System;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.CSharp;
using System.Diagnostics;
using TakeCommand.Plugin;

namespace CsScriptPlugin
{
    public class CsScript : ITCCPlugin
    {
        private readonly TCCPluginInfo _info = new TCCPluginInfo
        {
            Name        = "CsScript",
            Author      = "Joe Caverly",
            Email       = "jlcaverlyca@yahoo.ca",
            WWW         = "https://github.com/joec4281?tab=repositories",
            Description = "CsScript command to run CSharp code",
            Functions = "CSSCRIPT",

            Major = 2026,
            Minor       = 07,
            Build       = 21
        };

        public TCCPluginInfo GetPluginInfo() => _info;

        public bool Initialize()
        {
            return true;
        }

        public bool Shutdown(bool endProcess)
        {
            // Nothing to clean up
            return true;
        }

        /// <summary>
        /// Invokes a TCC command and returns its captured output.
        /// Available to .csx scripts as CsScriptPlugin.CsScript.TccRun(command).
        /// </summary>
        public static string TccRun(string command)
        {
            // Prefer direct compile-time call when TC-DotNetPluginHost64.dll is referenced.
            try
            {
                return TakeCommand.PluginHost.InvokeCommand(command);
            }
            catch (TypeLoadException) { /* host type not available at runtime - fallback to reflection */ }
            catch (MissingMethodException) { /* API mismatch - fallback to reflection */ }
            catch (Exception) { /* any other failure - attempt reflection fallback */ }

            // Reflection fallback: search loaded assemblies for the host and invoke InvokeCommand.
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = a.GetName().Name;
                if (name == "TC-DotNetPluginHost64" || name.StartsWith("TC-DotNetPluginHost"))
                {
                    var hostType = a.GetType("TakeCommand.PluginHost") ?? a.GetType("TakeCommand.Plugin.PluginHost");
                    if (hostType != null)
                    {
                        var invoke = hostType.GetMethod("InvokeCommand", new[] { typeof(string) });
                        if (invoke != null)
                            return (string)invoke.Invoke(null, new object[] { command });
                    }
                }
            }

            throw new InvalidOperationException("TccRun: TC-DotNetPluginHost64 not found in AppDomain.");
        }

        /// <summary>
        /// Command: CSSCRIPT filename.csx [arg1 arg2 arg3 ...]
        /// Loads and executes any .csx or .cs file with optional arguments.
        /// Usage: CSSCRIPT template.csx
        ///        CSSCRIPT template.csx 10 20 
        ///        CSSCRIPT myscript.cs param1 param2
        /// </summary>
        public int CSSCRIPT(StringBuilder args)
        {
                try
                {
                        string input = args.ToString().Trim();
//                      OutputDebugString(input);
                        if (string.IsNullOrEmpty(input))
                        {
                                Console.Error.WriteLine("CSSCRIPT error: filename required");
                                Console.Error.WriteLine("Usage: CSSCRIPT filename.csx [arg1 arg2 ...]");
                                return 1;
                        }

                        // Split the input: first token is filename, rest are arguments
                        string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string filename = parts[0];
                        string[] scriptArgs = new string[parts.Length - 1];
                        Array.Copy(parts, 1, scriptArgs, 0, parts.Length - 1);

                        // Resolve the file path
                        string scriptPath = Path.IsPathRooted(filename) 
                                ? filename 
                                : Path.Combine(Environment.CurrentDirectory, filename);

                        if (!File.Exists(scriptPath))
                        {
                                Console.Error.WriteLine($"CSSCRIPT error: file not found at {scriptPath}");
                                return 1;
                        }

                        // Read the script file
                        string scriptCode = File.ReadAllText(scriptPath);

                        // Compile the script
                        var csharpProvider = new CSharpCodeProvider();
                        var compilerParams = new CompilerParameters
                        {
                                GenerateExecutable = true,
                                GenerateInMemory = true
                        };

                        // Add system assemblies
                        compilerParams.ReferencedAssemblies.Add("System.dll");
                        compilerParams.ReferencedAssemblies.Add("System.Core.dll");
                        compilerParams.ReferencedAssemblies.Add(typeof(CsScript).Assembly.Location);

                        CompilerResults results = csharpProvider.CompileAssemblyFromSource(compilerParams, scriptCode);

                        if (results.Errors.Count > 0)
                        {
                                Console.Error.WriteLine("CSSCRIPT compilation errors:");
                                foreach (CompilerError error in results.Errors)
                                {
                                        Console.Error.WriteLine($"  Line {error.Line}: {error.ErrorText}");
                                }
                                return 1;
                        }

                        // Get the Program class and invoke Main
                        var programType = results.CompiledAssembly.GetType("Program");
                        if (programType == null)
                        {
                                Console.Error.WriteLine("CSSCRIPT error: Program class not found in script");
                                return 1;
                        }

                        // Try static private first (default for Main), then public
                        var mainMethod = programType.GetMethod("Main", 
                                BindingFlags.Static | 
                                BindingFlags.NonPublic);

                        if (mainMethod == null)
                        {
                                mainMethod = programType.GetMethod("Main", 
                                        BindingFlags.Public | 
                                        BindingFlags.Static);
                        }

                        if (mainMethod == null)
                        {
                                Console.Error.WriteLine("CSSCRIPT error: Main method not found in Program class");
                                return 1;
                        }

                        // Re-open stdout to honour TCC output redirection (e.g. CSSCRIPT foo.csx > clip:).
                        // Console.Out is cached at first use; OpenStandardOutput() re-reads the current
                        // Win32 stdout handle, which TCC may have changed via SetStdHandle before this call.
                        TextWriter savedOut = Console.Out;
                        Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding) { AutoFlush = true });

                        // Invoke Main with parsed arguments
                        try
                        {
                                if (mainMethod.GetParameters().Length > 0)
                                {
                                        mainMethod.Invoke(null, new object[] { scriptArgs });
                                }
                                else
                                {
                                        mainMethod.Invoke(null, null);
                                }
                                return 0;
                        }
                        catch (System.Reflection.TargetInvocationException ex)
                        {
                                Console.Error.WriteLine($"CSSCRIPT execution error: {ex.InnerException?.Message}");
                                return 1;
                        }
                        finally
                        {
                                Console.Out.Flush();
                                Console.SetOut(savedOut);
                        }
                }
                catch (Exception ex)
                {
                        Console.Error.WriteLine($"CSSCRIPT error: {ex.Message}");
                        return 1;
                }
        }
    }
}
