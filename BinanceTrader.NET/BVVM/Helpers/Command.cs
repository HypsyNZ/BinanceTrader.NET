/*
*MIT License
*
*Copyright (c) 2022 S Christison
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:
*
*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
*SOFTWARE.
*/

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.Helpers
{
    internal static class Command
    {
        private const int MAX_WAIT_TIME = 20000;

        /// <summary>
        /// Success Exit Codes
        /// </summary>
        private static readonly int[] successExitCodes = { 0, 1056 };

        /// <summary>
        /// Run a Command to the Local Terminal (Command Prompt) as Administrator, Detatching from the Parent Task if required
        /// <para>Standard Outputs can be redirected (will run detatched)</para>
        /// <para>Detatching will prevent thread blocking</para>
        /// </summary>
        /// <param name="commandString">The command as a string</param>
        /// <param name="detatchTask">Whether the command should detatch from the current process</param>
        /// <param name="redirectStandardOutputs">bool indicating whether output should be redirected to the parent process</param>
        /// <returns></returns>
        public static async Task<bool> RunAsync(string commandString, bool detatchTask = false, bool redirectStandardOutputs = false)
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
            var runCommand = new ProcessStartInfo
            {
                WorkingDirectory = @"C:\Windows\System32",
                FileName = @"C:\Windows\System32\cmd.exe",
                Verb = "runas",
                Arguments = "/c " + commandString,
                WindowStyle = ProcessWindowStyle.Hidden
            };

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

            r.WaitForExit(MAX_WAIT_TIME);
            if (successExitCodes.Contains(r.ExitCode))
            {
                return true;
            }

            return false;
        }
    }
}
