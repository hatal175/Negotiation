using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.App_Start
{
    public class SideConfig
    {
        public String Side { get; set; }
        public String Variant { get; set; }
    }

    public static class NegotiationConfig
    {
        public static int TotalRounds = 15;
        public static TimeSpan RoundLength = new TimeSpan(0,2,0);

        static void LoadDbData() 
        {

        }

        public static SideConfig GetHumanConfig()
        {
            return null;
        }

        public static SideConfig GetAiConfig()
        {
            return null;
        }
    }
}