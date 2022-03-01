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
using BTNET.ViewModels;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Media;
using System.Threading.Tasks;

namespace BTNET.BVVM.HELPERS
{
    internal class Helpers : ObservableObject
    {
        public static void PlaySound()
        {
            _ = Task.Run(() =>
              {
                  SoundPlayer sp = new SoundPlayer(Resource.bell);
                  sp.Play();
                  sp.Dispose();
              }).ConfigureAwait(false);
        }

        public static void PlayErrorSound()
        {
            _ = Task.Run(() =>
              {
                  SoundPlayer sp = new SoundPlayer(Resource.piezo);
                  sp.Play();
                  sp.Dispose();
              }).ConfigureAwait(false);
        }

        private static readonly ObservableCollection<BinanceSymbolViewModel> emptycollection = null;

        public static ObservableCollection<BinanceSymbolViewModel> EmptyCollection => emptycollection;

        /// <summary>
        /// Trim the zeros from the end of a <seealso cref="decimal"/>
        /// </summary>
        /// <param name="value"><seealso cref="decimal"/> to trim</param>
        /// <returns></returns>
        public static decimal TrimDecimal(decimal value)
        {
            if (value != 0)
            {
                string text = value.ToString(CultureInfo.InvariantCulture).TrimEnd('0');

                bool convertback = decimal.TryParse(text, out decimal outD);

                return convertback ? outD : 0;
            }
            return 0;
        }
    }
}