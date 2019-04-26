using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Tests.Binance
{
    public class BinanceCandleData
    {
        //[
        //  1499040000000,      // Open time
        //  "0.01634790",       // Open
        //  "0.80000000",       // High
        //  "0.01575800",       // Low
        //  "0.01577100",       // Close
        //  "148976.11427815",  // Volume
        //  1499644799999,      // Close time
        //  "2434.19055334",    // Quote asset volume
        //  308,                // Number of trades
        //  "1756.87402397",    // Taker buy base asset volume
        //  "28.46694368",      // Taker buy quote asset volume
        //  "17928899.62484339" // Ignore.
        //]

        public readonly DateTime OpenTime;
        public readonly decimal Open;
        public readonly decimal High;
        public readonly decimal Low;
        public readonly decimal Close;
        public readonly decimal Volume;
        public readonly DateTime CloseTime;


        public BinanceCandleData(string [] rawData)
        {
            OpenTime = UnixTimeConverter.JavaTimeStampToDateTime(Convert.ToInt64(rawData[0]));
            Open = new decimal(Convert.ToDouble(rawData[1],CultureInfo.InvariantCulture));
            High = new decimal(Convert.ToDouble(rawData[2], CultureInfo.InvariantCulture));
            Low = new decimal(Convert.ToDouble(rawData[3], CultureInfo.InvariantCulture));
            Close = new decimal(Convert.ToDouble(rawData[4], CultureInfo.InvariantCulture));
            Volume = new decimal(Convert.ToDouble(rawData[5], CultureInfo.InvariantCulture));
            CloseTime = UnixTimeConverter.JavaTimeStampToDateTime(Convert.ToInt64(rawData[6]));
        }
    }
}
