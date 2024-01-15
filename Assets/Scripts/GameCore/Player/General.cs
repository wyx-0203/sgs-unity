// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// namespace GameCore
// {
//     [Serializable]
//     public class General
//     {
//         // 编号
//         public int id;
//         // 势力
//         public string nation;
//         // 姓名
//         public string name;
//         // 性别
//         public bool gender;
//         // 体力上限
//         public int hp_limit;
//         // 技能
//         public List<string> skill;
//         public List<string> describe;

//         private static List<General> _list;
//         public static async Task<List<General>> GetList()
//         {
//             if (_list is null) _list = Model.JsonList<General>.FromJson(await WebRequest.Get(Url.JSON + "general.json"));
//             return _list;
//         }
//     }

//     [Serializable]
//     public class Skin
//     {
//         public int general_id;
//         public int id;
//         public string name;
//         public bool dynamic;
//         public List<Voice> voice;

//         private static List<Skin> _list;
//         public static async Task<List<Skin>> GetList()
//         {
//             if (_list is null) _list = Model.JsonList<Skin>.FromJson(await WebRequest.Get(Url.JSON + "skin.json"));
//             return _list;
//         }
//         public static async Task<IEnumerable<Skin>> GetList(int general_id)
//         {
//             if (_list is null) _list = Model.JsonList<Skin>.FromJson(await WebRequest.Get(Url.JSON + "skin.json"));
//             return _list.Where(x => x.general_id == general_id);
//         }
//     }

//     [Serializable]
//     public class Voice
//     {
//         public string name;
//         public List<string> url;
//     }
// }