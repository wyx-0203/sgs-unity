using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model
{
    // [Serializable]
    // public class JsonList<T>
    // {
    //     public List<T> list;

    //     public static List<T> FromJson(string json)
    //     {
    //         return JsonConvert.DeserializeObject<JsonList<T>>(json).list;
    //     }
    // }
    public static class JsonExtensions
    {
        public static T DeSerialize<T>(this string json) => JsonConvert.DeserializeObject<T>(json);
        public static Message DeSerialize(this string json)
        {
            var type = Type.GetType(json.DeSerialize<Message>()._type);
            return JsonConvert.DeserializeObject(json, type) as Message;
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
    public class JoinRoomResponse : HttpResponse
    {
        public string mode;
        // public int owner_pos;
        // public List<UserJson> players;
        public int room_id;
        public string room_url;

    }

    [Serializable]
    public class UserInfoResponse : HttpResponse
    {
        public string nickname;
        public string character;
        public int win;
        public int lose;
    }



    [Serializable]
    public class Message
    {
        public string _type;
        public int player = -1;
        public string text;

        public Message()
        {
            _type = GetType().ToString();
        }

        public string Serialize() => JsonConvert.SerializeObject(this);
    }


    // [Serializable]
    // public class AddPlayerMessage : Message
    // {
    //     public UserJson user;
    // }

    // [Serializable]
    // public class removePlayerMessage : Message
    // {
    //     public int position;
    //     public int owner_pos;
    // }


    // [Serializable]
    // public class ChatMessage : Message
    // {
    //     public int user_id;
    //     public string content;
    // }

}