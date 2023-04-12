using System;
// using System.Collections.Generic;
// using UnityEngine;

// public class User
// {
//     public int ID { get; set; }
//     public bool Already { get; set; }
// }
namespace Model
{
    [Serializable]
    public class User
    {
        public int id;
        public int position;
        public string nickname;
        public string character;
        public bool already;
        public bool owner;
    }
}