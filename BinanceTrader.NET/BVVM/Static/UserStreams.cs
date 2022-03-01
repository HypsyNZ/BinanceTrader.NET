using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class UserStreams : ObservableObject
    {
        public static Task CloseUserStreams()
        {
            if (Static.SpotListenKey != string.Empty)
            {
                _ = BTClient.Local.Spot.UserStream.StopUserStreamAsync(Static.SpotListenKey);
                Static.SpotListenKey = string.Empty;
            }

            if (Static.MarginListenKey != string.Empty)
            {
                _ = BTClient.Local.Margin.UserStream.StopUserStreamAsync(Static.MarginListenKey);
                Static.MarginListenKey = string.Empty;
            }

            if (Static.IsolatedListenKey != string.Empty && Static.LastIsolatedListenKeySymbol != string.Empty)
            {
                _ = BTClient.Local.Margin.IsolatedUserStream.CloseIsolatedMarginUserStreamAsync(Static.LastIsolatedListenKeySymbol, Static.IsolatedListenKey);
                Static.IsolatedListenKey = string.Empty;
            }

            return Task.CompletedTask;
        }

        public static async Task<bool> ResetUserStreamsOnError()
        {
            Static.chartBases = new();

            Static.SpotListenKey = string.Empty;

            MainVM.IsIsolated = false;
            Static.IsolatedListenKey = string.Empty;

            MainVM.IsMargin = false;
            Static.MarginListenKey = string.Empty;

            return await Search.SearchPricesUpdate().ConfigureAwait(false);
        }
    }
}