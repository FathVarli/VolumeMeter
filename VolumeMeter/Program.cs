using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;

namespace VolumeMeter
{
    class Program
    {
        const string Url = "https://agile-cliffs-23967.herokuapp.com/ok";

        static void Main(string[] args)
        {
            var coins = new List<Signal>();


            while (true)
            {
                var data = ApiRequest();
                var signalList = JsonConvert.DeserializeObject<VolumeMeter>(data);

                bool isEditing = false;

                if (signalList != null)
                {
                    var lastItem = signalList.Resu.Last();
                    signalList.Resu.Remove(lastItem);
                }


                if (signalList != null && signalList.Resu.Count > 0)
                    foreach (var signal in signalList.Resu)
                    {
                        var signalArr = signal.Split("|");
                        var newSignal = new Signal
                        {
                            Name = signalArr[0],
                            Ping = int.Parse(signalArr[1]),
                            NetVolBtc = signalArr[2],
                            NetVolPercent = $"{signalArr[3]}",
                            RecentTotalVolBtc = signalArr[4],
                            RecentVolPercent = $"{signalArr[5]}",
                            RecentNetVol = signalArr[6],
                            DateTime = Convert.ToDateTime(signalArr[7]),
                            Count = 1
                        };

                        if (newSignal.DateTime.Date > DateTime.UtcNow)
                        {
                            coins.Clear();
                        }
                        else
                        {
                            if (coins.Count < 1)
                            {
                                coins.Add(newSignal);
                                isEditing = true;
                            }
                            else
                            {
                                var last = coins.LastOrDefault(c => c.Name == newSignal.Name);
                                if (last != null && last.DateTime != newSignal.DateTime)
                                {
                                    newSignal.Count = last.Count + 1;
                                    coins.Add(newSignal);
                                    isEditing = true;
                                }
                                
                                if (coins.All(c => c.Name != newSignal.Name))
                                {
                                    coins.Add(newSignal);
                                    isEditing = true;
                                }
                            }
                        }


                        if (isEditing)
                        {

                            Console.WriteLine(
                                "Coin Adı: {0} Sinyal Sayısı: {1} Ping:{2} NetVolBtc: {3} NetVol%: {4} Recent Total Vol Btc: {5} Recent Vol%: {6}" +
                                " Recent Net Vol {7} Signal Time: {8}  ", newSignal.Name, newSignal.Count,
                                newSignal.Ping, newSignal.NetVolBtc, newSignal.NetVolPercent,
                                newSignal.RecentTotalVolBtc, newSignal.RecentVolPercent, newSignal.RecentNetVol,
                                newSignal.DateTime);
                        }
                    }

                Thread.Sleep(1000);
            }

            Console.ReadKey();
        }

        static string ApiRequest()
        {
            try
            {
                var request = WebRequest.Create(Url);
                request.Method = "GET";

                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();


                using var reader = new StreamReader(webStream);
                var data = reader.ReadToEnd();
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }

    class VolumeMeter
    {
        public List<string> Resu { get; set; }
    }

    class Signal
    {
        public string Name { get; set; }
        public int Ping { get; set; }
        public string NetVolBtc { get; set; }
        public string NetVolPercent { get; set; }
        public string RecentTotalVolBtc { get; set; }
        public string RecentVolPercent { get; set; }
        public string RecentNetVol { get; set; }
        public DateTime DateTime { get; set; }
        public int Count { get; set; }
    }
}