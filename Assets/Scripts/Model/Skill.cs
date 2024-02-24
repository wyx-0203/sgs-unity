using System;
using System.Collections.Generic;

namespace Model
{

    [Serializable]
    public class UpdateSkill : Message
    {
        public List<string> skills;
    }

    [Serializable]
    public class UseSkill : Message
    {
        public string skill;
        public List<int> dests;
    }
}