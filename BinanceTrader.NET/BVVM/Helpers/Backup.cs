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

using BTNET.BVVM.Log;
using System;
using System.IO;

namespace BTNET.BVVM.Helpers
{
    internal static class Backup
    {
        /// <summary>
        /// Restore Backup of given file
        /// <para>the original file will be deleted if the backup for it is located</para>
        /// </summary>
        /// <param name="originalFileFullPath">File you want to locate and restore the backup for</param>
        /// <param name="backupName">Friendly name of this backup for logging purposes</param>
        /// <returns></returns>
        public static bool RestoreBackup(string originalFileFullPath, string backupName)
        {
            try
            {
                var backup = originalFileFullPath + ".bak";

                if (File.Exists(backup))
                {
                    File.Delete(originalFileFullPath);

                    WriteLog.Error("Attemping to restore [" + backupName + "] from backup");

                    File.Copy(backup, originalFileFullPath);

                    return true;
                }
                else
                {
                    WriteLog.Error("No Backup of [" + backupName + "] to Restore");
                }

                return false;
            }
            catch (Exception ex)
            {
                WriteLog.Error("Exception while restoring backup: " + ex);
                return false;
            }
        }

        /// <summary>
        /// Backup the given file
        /// </summary>
        /// <param name="originalFileFullPath">File you want to create a backup of</param>
        public static bool SaveBackup(string originalFileFullPath)
        {
            // originalFile.bak
            var backup = originalFileFullPath + ".bak";

            try
            {
                // Delete Old Backup
                File.Delete(backup);

                // Replace it with the last successfully serialized backup
                File.Copy(originalFileFullPath, backup);

                WriteLog.Info("Saved Backup: " + backup);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.Error("Exception while saving backup: " + ex);
                return false;
            }
        }
    }
}
