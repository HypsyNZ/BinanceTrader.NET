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

using BTNET.BV.Converters;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using Newtonsoft.Json;
using System;
using System.IO;

namespace BTNET.BVVM.BT
{
    /// <summary>
    /// <typeparamref name="T"/>JSON File
    /// <para>Save <typeparamref name="T"/> as JSON to File</para>
    /// <para>Load JSON as <typeparamref name="T"/> into Memory</para>
    /// <para>Automatic Backup on Save/Restore on Load</para>
    /// </summary>
    public class TJson
    {
        public static JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new TypeOnlyContractResolver()
        };

        /// <summary>
        /// Save <typeparamref name="T"/> to File
        /// <para>will automatically create a valid backup</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectBeingStored">The <typeparamref name="T"/> to Store</param>
        /// <param name="filePath">file path to save the <typeparamref name="T"/> as JSON</param>
        /// <param name="jsonSerializerSettings"><see cref="JsonSerializerSettings"/> to use; if you don't specify <see cref="SerializerSettings"/> will be used</param>
        /// <returns>true if the <typeparamref name="T"/> was written to file and the backup was successfully created</returns>
        public static bool Save<T>(T objectBeingStored, string filePath, JsonSerializerSettings? jsonSerializerSettings = null)
        {
            if (jsonSerializerSettings == null)
            {
                jsonSerializerSettings = SerializerSettings;
            }
            try
            {
                if (objectBeingStored != null)
                {
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(objectBeingStored, jsonSerializerSettings));

                    string CheckValidityOfBackup = File.ReadAllText(filePath).Normalize();
                    var backup = JsonConvert.DeserializeObject<T>(CheckValidityOfBackup, jsonSerializerSettings);

                    if (backup != null)
                    {
                        return Backup.SaveBackup(filePath);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error Storing List: ", ex);
            }

            return false;
        }

        /// <summary>
        /// Load <typeparamref name="T"/> from file, Repairing it if required
        /// <para>Will automatically attempt to restore a backup when required</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">file path to load the <typeparamref name="T"/> from</param>
        /// <param name="shouldBeOneDimensional">true if repair should be attempted, will remove { } from the entire string</param>
        /// <param name="jsonSerializerSettings"><see cref="JsonSerializerSettings"/> to use; if you don't specify <see cref="SerializerSettings"/> will be used</param>
        /// <returns>The deserialized <typeparamref name="T"/> or default(<typeparamref name="T"/>)</returns>
        public static T? Load<T>(string filePath, bool shouldBeOneDimensional = false, JsonSerializerSettings? jsonSerializerSettings = null)
        {
            if (jsonSerializerSettings == null)
            {
                jsonSerializerSettings = SerializerSettings;
            }

            if (File.Exists(filePath))
            {
                bool attempt = false;

                try
                {
                    return DeserializedObjectOrDefault<T>(shouldBeOneDimensional, filePath, jsonSerializerSettings);
                }
                catch
                {
                    attempt = Backup.RestoreBackup(filePath, filePath);
                }

                WriteLog.Error(attempt ? "Restored: " + filePath : "Restore Failed: " + filePath);

                if (attempt)
                {
                    try
                    {
                        return DeserializedObjectOrDefault<T>(shouldBeOneDimensional, filePath, jsonSerializerSettings);
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Error("Error While Deserializing JSON: " + ex);
                    }
                }
            }

            return default(T);
        }

        public static T? DeserializedObjectOrDefault<T>(bool shouldBeOneDimensional, string filePath, JsonSerializerSettings? jsonSerializerSettings = null)
        {
            if (jsonSerializerSettings == null)
            {
                jsonSerializerSettings = SerializerSettings;
            }

            string serializedText = File.ReadAllText(filePath).Normalize();
            if (serializedText != null && serializedText != "")
            {
                if (shouldBeOneDimensional && (serializedText.StartsWith("{\"List\":[") || serializedText.StartsWith("{[")))
                {
                    serializedText = serializedText.Replace("{\"List\":[", "[").Replace("{[\"", "\"[").Replace("\"]}", "\"]").Replace("}", "").Replace("{", "");
                    File.WriteAllText(filePath, serializedText);
                    WriteLog.Error("Repaired: " + filePath);
                }

                var deserializedObject = JsonConvert.DeserializeObject<T>(serializedText, jsonSerializerSettings);
                if (deserializedObject != null)
                {
                    return deserializedObject;
                }
            }

            return default(T);
        }
    }
}
