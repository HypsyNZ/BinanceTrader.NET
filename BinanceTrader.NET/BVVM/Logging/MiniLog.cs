//******************************************************************************************************
//  Copyright © 2022, S. Christison. No Rights Reserved.
//
//  Licensed to [You] under one or more License Agreements.
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//
//******************************************************************************************************

using BTNET.BVVM.HELPERS;

namespace BTNET.BVVM
{
    /// <summary>
    /// A Simple Static Log that can be used anywhere in your application to output a string to a Textbox/Textblock etc
    /// </summary>
    public class MiniLog
    {
        private static string minilogstring = "";

        /// <summary>
        /// Bindable <see cref="MiniLogString"/>
        /// <para>Bind this to a Textblock or Textbox</para>
        /// </summary>
        public static string MiniLogString { get => minilogstring; set => minilogstring = value; }

        /// <summary>
        /// Add a Line to the <see cref="MiniLog"/> string
        /// By default Newline is automatic and at the end of the string
        /// </summary>
        /// <param name="message">The string you want to add</param>
        /// <param name="newline">True for New Line [Default True]</param>
        /// <param name="newlinepos">True for End of String, False for Start of String [Default True]</param>
        public static void AddLine(string message, bool newline = true, bool newlinepos = true)
        {
            Invoke.InvokeUI(() =>
            {
                if (newline)
                {
                    if (newlinepos)
                    {
                        MiniLogString = MiniLogString + message + "\n";
                    }
                    else
                    {
                        MiniLogString = "\n" + MiniLogString + message;
                    }
                }
                else
                {
                    MiniLogString += message;
                }
            });
        }
    }
}