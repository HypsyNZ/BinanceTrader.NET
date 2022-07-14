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

using BTNET.BV.Enum;

namespace BTNET.BVVM.BT
{
    public struct State
    {
        private MonitorState state;

        public void SetWaiting()
        {
            state = MonitorState.Waiting;
        }

        public void SetWorking()
        {
            state = MonitorState.Working;
        }

        public void SetCompleted()
        {
            state = MonitorState.Completed;
        }

        public void SetError()
        {
            state = MonitorState.Error;
        }

        public bool IsWaiting()
        {
            return state == MonitorState.Waiting;
        }

        public bool IsWorking()
        {
            return state == MonitorState.Working;
        }

        public bool IsCompleted()
        {
            return state == MonitorState.Completed;
        }

        public bool IsError()
        {
            return state == MonitorState.Error;
        }

        public MonitorState GetState()
        {
            return state;
        }
    }
}
