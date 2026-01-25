using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PuaSuiteClean.Logic
{
    public class LoaderBuilder
    {
        private readonly string outputRoot =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "loaders");

        public string BuildLoader(string targetFile, LanguageType type)
        {
            string safeName = Path.GetFileNameWithoutExtension(targetFile);
            string loaderDir = Path.Combine(outputRoot, safeName);

            if (!Directory.Exists(loaderDir))
                Directory.CreateDirectory(loaderDir);

            string targetCopy = Path.Combine(loaderDir, Path.GetFileName(targetFile));
            File.Copy(targetFile, targetCopy, true);

            string loaderExe = Path.Combine(loaderDir, safeName + "_loader.exe");
            BuildStubExecutable(loaderExe, targetCopy);

            return loaderDir;
        }

        private void BuildStubExecutable(string exePath, string targetCopy)
        {
            string code = GenerateStubCode(targetCopy);

            string tempDir = Path.Combine(Path.GetTempPath(), "PuaLiteStub");
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            string csFile = Path.Combine(tempDir, "stub.cs");
            File.WriteAllText(csFile, code, Encoding.UTF8);

            string csc = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "dotnet", "sdk", "9.0.100", "Roslyn", "bincore", "csc.dll"
            );

            if (!File.Exists(csc))
                throw new Exception("Roslyn compiler not found");

            Process p = new Process();
            p.StartInfo.FileName = "dotnet";
            p.StartInfo.Arguments = $"\"{csc}\" /target:exe /out:\"{exePath}\" \"{csFile}\"";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.Start();
            p.WaitForExit();

            if (!File.Exists(exePath))
                throw new Exception("Loader build failed.");
        }

        private string GenerateStubCode(string target)
        {
            string escaped = target.Replace("\\", "\\\\").Replace("\"", "\\\"");

            return $@"
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

class LoaderLite
{{
    [DllImport(""kernel32.dll"")]
    static extern IntPtr OpenProcess(int access, bool inherit, int pid);

    [DllImport(""kernel32.dll"", SetLastError=true)]
    static extern IntPtr VirtualAllocEx(IntPtr proc, IntPtr addr, uint size, uint allocType, uint protect);

    [DllImport(""kernel32.dll"", SetLastError=true)]
    static extern bool WriteProcessMemory(IntPtr proc, IntPtr addr, byte[] buffer, uint size, out IntPtr written);

    [DllImport(""kernel32.dll"")]
    static extern IntPtr GetModuleHandle(string lib);

    [DllImport(""kernel32.dll"")]
    static extern IntPtr GetProcAddress(IntPtr module, string name);

    [DllImport(""kernel32.dll"")]
    static extern IntPtr CreateRemoteThread(IntPtr proc, IntPtr attr, uint stack, IntPtr start, IntPtr param, uint flags, IntPtr tid);

    const int PROCESS_ALL_ACCESS = 0x1F0FFF;

    static void Main()
    {{
        string target = ""{escaped}"";
        Console.WriteLine(""[Loader] Launching target: "" + target);

        if (!File.Exists(target))
        {{
            Console.WriteLine(""[Loader] ERROR: Target not found."");
            return;
        }}

        var psi = new ProcessStartInfo
        {{
            FileName = target,
            UseShellExecute = true,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal
        }};

        Process p = Process.Start(psi);
        Console.WriteLine(""[Loader] Process ID: "" + p.Id);

        System.Threading.Thread.Sleep(600);

        string dll = Path.Combine(Path.GetDirectoryName(target), ""inject.dll"");

        if (File.Exists(dll))
        {{
            Console.WriteLine(""[Loader] Injecting: "" + dll);
            InjectDLL(p.Id, dll);
        }}
        else
        {{
            Console.WriteLine(""[Loader] No DLL found, skipping injection."");
        }}

        Console.WriteLine(""[Loader] Done."");
    }}

    static void InjectDLL(int pid, string dll)
    {{
        try
        {{
            IntPtr h = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
            if (h == IntPtr.Zero)
            {{
                Console.WriteLine(""[Loader] ERROR: OpenProcess failed."");
                return;
            }}

            byte[] bytes = Encoding.ASCII.GetBytes(dll + ""\0"");

            IntPtr addr = VirtualAllocEx(h, IntPtr.Zero, (uint)bytes.Length, 0x3000, 0x40);
            WriteProcessMemory(h, addr, bytes, (uint)bytes.Length, out _);

            IntPtr load = GetProcAddress(GetModuleHandle(""kernel32.dll""), ""LoadLibraryA"");
            CreateRemoteThread(h, IntPtr.Zero, 0, load, addr, 0, IntPtr.Zero);

            Console.WriteLine(""[Loader] DLL injected successfully."");
        }}
        catch (Exception ex)
        {{
            Console.WriteLine(""Injection failed: "" + ex.Message);
        }}
    }}
}}
";
        }
    }
}
