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
        private string notepadCurrentText = "";
        public ICommand? SaveNotesCommand { get; set; }

        public string NotepadCurrentText
        {
            get => notepadCurrentText;
            set
            {
                notepadCurrentText = value;
                PC();
            }
        }

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
            File.WriteAllText(App.StoredNotes, NotepadCurrentText);
        }

        public void LoadNotes()
        {
            try
            {
                if (File.Exists(App.StoredNotes))
                {
                    string notes = File.ReadAllText(App.StoredNotes);
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
            finally
            {
                MainVM.NotepadReady = true;
            }
        }
    }
}
