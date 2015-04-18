using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public class QuestionModel
    {
        public QuestionModel()
        {

        }

        public String Question { get; set; }
        public List<String> Options { get; set; }
        public String Answer { get; set; }
        public String ActualAnswer { get; set; }
    }

    public class NegotiationTutorialModel
    {
        public String Id { get; set; }
        public List<QuestionModel> Questions { get; set; }
    }
}