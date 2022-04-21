using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.Helpers
{
    internal static class Command
    {
        /// <summary>
        /// Success Exit Codes
        /// </summary>
        private static int[] successExitCodes = { 0, 1056 };

        /// <summary>
        /// Run a Command to the Local Terminal (Command Prompt) as Administrator, Detatching from the Parent Task if required
        /// <para>Standard Outputs can be redirected (will run detatched)</para>
        /// <para>Detatching will prevent thread blocking</para>
        /// </summary>
        /// <param name="commandString">The command as a string</param>
        /// <param name="detatchTask">Whether the command should detatch from the current process</param>
        /// <param name="redirectStandardOutputs">bool indicating whether output should be redirected to the parent process</param>
        /// <returns></returns>
        public static async Task<bool> Run(string commandString, bool detatchTask = false, bool redirectStandardOutputs = false)
        {
            if (detatchTask || redirectStandardOutputs)
            {
                Task<bool> k = await Task.Factory.StartNew(() =>
                {
                    return Task.FromResult(CommandRunner(commandString, redirectStandardOutputs));
                }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);

                return k.Result;
            }

            return CommandRunner(commandString);
        }

        /// <summary>
        /// Runs the Command to the Local Terminal (Command Prompt) as Administrator
        /// <para>runas</para>
        /// </summary>
        /// <param name="commandString">The command as a string</param>
        /// <param name="redirectStandardOutputs">bool indicating whether output should be redirected to the parent process</param>
        /// <returns></returns>
        private static bool CommandRunner(string commandString, bool redirectStandardOutputs = false)
        {
            var runCommand = new ProcessStartInfo();

            runCommand.WorkingDirectory = @"C:\Windows\System32";
            runCommand.FileName = @"C:\Windows\System32\cmd.exe";
            runCommand.Verb = "runas";
            runCommand.Arguments = "/c " + commandString;
            runCommand.WindowStyle = ProcessWindowStyle.Hidden;

            if (redirectStandardOutputs)
            {
                runCommand.RedirectStandardError = true;
                runCommand.RedirectStandardOutput = true;
                runCommand.UseShellExecute = false;
            }
            else
            {
                runCommand.UseShellExecute = true;
            }

            var r = Process.Start(runCommand);

            if (redirectStandardOutputs)
            {
                r.BeginErrorReadLine();
                r.BeginOutputReadLine();
            }

            r.WaitForExit(20000);
            if (successExitCodes.Contains(r.ExitCode))
            {
                return true;
            }

            return false;
        }
    }
}