using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    [Serializable]
    public class General
    {
        // 编号
        public int id;
        // 势力
        public string nation;
        // 姓名
        public string name;
        // 性别
        public bool gender;
        // 体力上限
        public int hp_limit;
        // 技能
        public List<string> skill;
        public List<string> describe;
        // 皮肤
        // public List<Skin> skins;

        public static async Task<List<General>> GetList()
        {
            return JsonList<General>.FromJson(await WebRequest.Get(Url.JSON + "general.json"));
        }
    }

    [Serializable]
    public class Skin
    {
        public int id;
        public string name;
        public List<Voice> voice;
    }

    [Serializable]
    public class Voice
    {
        public string name;
        public List<string> url;
    }
}