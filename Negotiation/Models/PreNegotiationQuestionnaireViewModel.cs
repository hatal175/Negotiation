using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    //public enum AgeRange
    //{
    //    [Description("15-20")]
    //    FifteenToTwenty,

    //    [Description("21-30")]
    //    TwentyOneToThirty,

    //    [Description("31+")]
    //    ThirtyPlus
    //}

    public static class PreNegotiationQuestionnaireUtils
    {
        public static String Description(this AgeRange ageRange)
        {
            switch (ageRange)
            {
                case AgeRange.FifteenToTwenty:
                    return "15-20";
                case AgeRange.TwentyOneToThirty:
                    return "21-30";
                case AgeRange.ThirtyOnePlus:
                    return "31+";
                default:
                    return "No Description";
            }
        }

        public static String Description(this Education education)
        {
            switch (education)
            {
                case Education.TwelveYears:
                    return "High School Education";
                case Education.Bachelor:
                    return "Bachelor Degree";
                case Education.Masters:
                    return "Masters Degree";
                case Education.Phd:
                    return "Phd";
                default:
                    return "No Description";
            }
        }
    }

    public class PreNegotiationQuestionnaireViewModel
    {
        [Required]
        public Gender Gender { get; set; }

        [Required]
        public AgeRange AgeRange { get; set; }

        [Required]
        public Education Education { get; set; }

        public String DegreeField { get; set; }

        [Required]
        public String BirthCountry { get; set; }

        [Required]
        public String Name { get; set; }

        [Required]
        public int ID { get; set; }

        [Required]
        public String University { get; set; }

        [Required]
        public bool AgreeIRB { get; set; }
    }
}