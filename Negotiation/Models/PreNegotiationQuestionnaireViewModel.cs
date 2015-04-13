using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public enum Gender
    {
        Male,
        Female
    }

    public enum AgeRange
    {
        [Description("15-20")]
        FifteenToTwenty,

        [Description("21-30")]
        TwentyOneToThirty,

        [Description("31+")]
        ThirtyPlus
    }
    
    public enum Education
    {
        [Description("High School Education")]
        TwelveYears,

        [Description("Bachelor Degree")]
        Bachelor,

        [Description("Masters Degree")]
        Masters,

        [Description("Phd")]
        Phd
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