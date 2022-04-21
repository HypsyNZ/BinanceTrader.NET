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

using BTNET.BV.Resources;
using BTNET.BVVM.Log;
using System;
using System.Media;
using System.Threading.Tasks;

namespace BTNET.BVVM.Helpers
{
    internal class Sound : ObservableObject
    {
        public static void PlaySound()
        {
            try
            {
                _ = Task.Run(() =>
                  {
                      SoundPlayer sp = new SoundPlayer(Resource.bell);
                      sp.Play();
                      sp.Dispose();
                  }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Something went wrong playing the Bell Sound", ex);
            }
        }

        public static void PlayErrorSound()
        {
            try
            {
                _ = Task.Run(() =>
                  {
                      SoundPlayer sp = new SoundPlayer(Resource.piezo);
                      sp.Play();
                      sp.Dispose();
                  }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Something went wrong playing the Error Sound", ex);
            }
        }
    }
}