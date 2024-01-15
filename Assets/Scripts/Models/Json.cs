using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace Model
{
    [Serializable]
    public class JsonList<T>
    {
        public List<T> list;

        public static List<T> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<JsonList<T>>(json).list;
        }
    }

    [Serializable]
    public class HttpResponse
    {
        public int code;
        public string message;
    }

    [Serializable]
    public class SignInResponse : HttpResponse
    {
        public string token;
        public int user_id;
    }


    [Serializable]
    public class UserJson
    {
        public int id;
        public int position;
        public string nickname;
        public string character;
        public bool already;
        public bool owner;
    }

    [Serializable]
    public class JoinRoomResponse : HttpResponse
    {
        public string mode;
        public int owner_pos;
        public List<UserJson> players;

    }

    [Serializable]
    public class UserInfoResponse : HttpResponse
    {
        public string nickname;
        public string character;
        public int win;
        public int lose;
    }

    // [Serializable]
    // public class Card
    // {
    //     public int id;
    //     public string suit;
    //     public int weight;
    //     public string type;
    //     public string name;
    // }

    // [Serializable]
    // public class Message
    // {
    // }

    [Serializable]
    public class AddPlayerMessage : Message
    {
        public UserJson user;
    }

    [Serializable]
    public class removePlayerMessage : Message
    {
        public int position;
        public int owner_pos;
    }

    [Serializable]
    public class SetAlreadyMessage : Message
    {
        public int position;
        public bool already;
    }

    [Serializable]
    public class Surrender : Message
    {
        // public Team team;
        public Surrender() { }
    }

    [Serializable]
    public class GeneralPoolMessage : Message
    {
        public List<int> generals;
    }

    [Serializable]
    public class BanpickMessage : Message
    {
        // public int position;
        public Team team;
        public List<int> generals;
    }

    // [Serializable]
    // public class PhaseMessage : WebsocketMessage
    // {
    //     public int position;
    //     public GameCore.Phase phase;
    // }

    [Serializable]
    public class Decision : Message
    {
        public int _id;
    }

    [Serializable]
    public class Shuffle : Decision
    {
        public List<int> cards;
    }

    // [Serializable]
    // public class ChangeSkin : Message
    // {
    //     // public int position;
    //     public int skin_id;
    // }

    [Serializable]
    public class ChatMessage : Message
    {
        public int user_id;
        public string content;
    }

}