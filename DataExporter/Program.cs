using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Negotiation.Models;

namespace DataExporter
{

    class Program
    {
        static void Main(string[] args)
        {


            var columns = new List<OutputColumn>()
            {
                new OutputColumn("Strategy Utility", game => game.Users.First(user => user.Strategy != null).Score.ToString()),
                new OutputColumn("Opponent Utility", game => game.Users.First(user => user.Strategy == null).Score.ToString()),
                new OutputColumn("End Reason", game => game.NegotiationActions.Last().Type.ToString()),
                new OutputColumn("Game Time", game => (TimeSpan.FromMinutes(30) - game.NegotiationActions.Last().RemainingTime).TotalSeconds.ToString())
            };

            try
            {
                using (StreamWriter sw = new StreamWriter("output.csv"))
                {
                    sw.WriteLine(String.Join(",", columns.Select(x => x.Name)));

                    NegotiationContainer cont =
                        new NegotiationContainer(
                         //   "metadata=res://*/Models.Negotiation.csdl|res://*/Models.Negotiation.ssdl|res://*/Models.Negotiation.msl;provider=System.Data.SqlClient;provider connection string=&quot;Data Source=SQL5003.Smarterasp.net;Initial Catalog=DB_9BA48E_negotiation;User Id=DB_9BA48E_negotiation_admin;Password=db1negotiation;&quot;"
                            );

                    foreach (
                        var game in
                            cont.GameSet.Where(x => x.Endtime != null)
                                .Where(x => x.StartTime > new DateTime(2015, 8, 1)))
                    {
                        sw.WriteLine(String.Join(",", columns.Select(x => x.Func(game))));
                    }
                }
            }
            catch (Exception ex)
            {
                int j = 4;
            }
            


        }
    }
}
