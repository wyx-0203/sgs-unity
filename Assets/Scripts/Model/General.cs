using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Model
{
    public enum Gender
    {
        Female = 0,
        Male = 1
    }

    [Serializable]
    public class General
    {
        // 编号
        public int id;
        // 势力
        public string kindom;
        // 姓名
        public string name;
        // 性别
        public Gender gender;
        // 体力上限
        public int hpLimit;
        public int hp;
        // 技能
        public List<string> skills;
        public List<int> skins;

        private static List<General> _list;
        public static void Init(string json)
        {
            _list = JsonConvert.DeserializeObject<List<General>>(json);
        }
        public static List<General> GetList() => _list;
        public static General Get(int id) => _list.Find(x => x.id == id);
    }
}