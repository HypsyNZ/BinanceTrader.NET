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

using BTNET.BVVM;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.IO;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class NotepadViewModel : ObservableObject
    {
        private string notepadCurrentText;
        public ICommand SaveNotesCommand { get; set; }

        public string NotepadCurrentText
        { get => notepadCurrentText; set { notepadCurrentText = value; PC(); } }

        public void InitializeCommands()
        {
            SaveNotesCommand = new DelegateCommand(Save);
        }

        private void Save(object o)
        {
            SaveNotes();
        }

        public void SaveNotes()
        {
            File.WriteAllText(Stored.storedNotes, NotepadCurrentText);
        }

        public void LoadNotes()
        {
            try
            {
                if (File.Exists(Stored.storedNotes))
                {
                    string notes = File.ReadAllText(Stored.storedNotes);
                    if (notes != null && (notes != "" || notes != string.Empty))
                    {
                        NotepadCurrentText = notes;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }
        }
    }
}