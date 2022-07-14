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

using System;
using System.Windows.Input;

namespace BTNET.BVVM.Helpers
{
    public class DelegateCommand : ICommand
    {
        /// <summary>
        /// Occurs when the Action of a Command has become Null for some exceptional reason
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        private volatile Action<object> _action;
        private bool _canExecute = true;

        public DelegateCommand(Action<object> action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        public void AddAction(Action<object> action)
        {
            if (_action == null)
            {
                _action = action;
            }
        }

        public bool CanExecute(object parameter)
        {
            var t = _canExecute;

            if (_action != null)
            {
                _canExecute = true;
            }
            else
            {
                _canExecute = false;
            }

            if (t != _canExecute)
            {
                CanExecuteChanged?.Invoke(_canExecute, null);
            }

            return _canExecute;
        }
    }
}
