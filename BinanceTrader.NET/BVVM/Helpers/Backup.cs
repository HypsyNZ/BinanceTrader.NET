using BTNET.BVVM.Log;
using System.IO;

namespace BTNET.BVVM.Helpers
{
    internal class Backup
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
            var backup = originalFileFullPath + ".bak";

            if (File.Exists(backup))
            {
                File.Delete(originalFileFullPath);

                WriteLog.Error("Attemping to restore " + backupName + " from backup");

                File.Copy(backup, originalFileFullPath);
            }
            else
            {
                WriteLog.Error("No Backup of " + backupName + " to Restore");
            }

            // attempt complete
            return true;
        }

        /// <summary>
        /// Backup the given file
        /// </summary>
        /// <param name="originalFileFullPath">File you want to create a backup of</param>
        public static void SaveBackup(string originalFileFullPath)
        {
            // originalFile.bak
            var backup = originalFileFullPath + ".bak";

            // Delete Old Backup
            File.Delete(backup);

            // Replace it with the last successfully serialized Orders
            File.Copy(originalFileFullPath, backup);
        }
    }
}