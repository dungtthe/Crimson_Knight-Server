using Crimson_Knight_Server.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players
{
    public class Quest
    {
        public int Id { get; set; }
        public int QuantityCur {  get; set; }
        public QuestState QuestState { get; set; }
        public QuestTemplate Template { get; set; }
    }
    public enum QuestState
    {
        NotAccepted,
        InProgress,  
        Completed
    }
}
