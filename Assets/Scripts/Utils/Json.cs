using System.Collections.Generic;
using System;
using UnityEngine;

public class JsonList<T>
{
    public List<T> list;

    public static List<T> FromJson(string json)
    {
        return JsonUtility.FromJson<JsonList<T>>(json).list;
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
    public Mode mode;
    public int owner_pos;
    public List<Model.User> players;

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
public class CardJson
{
    public int id;
    public string suit;
    public int weight;
    public string type;
    public string name;
}

[Serializable]
public class WebsocketMessage
{
    public string msg_type;
}

[Serializable]
public class AddPlayerMessage : WebsocketMessage
{
    public Model.User player;
}

[Serializable]
public class removePlayerMessage : WebsocketMessage
{
    public int position;
    public int owner_pos;
}

[Serializable]
public class SetAlreadyMessage : WebsocketMessage
{
    public int position;
    public bool already;
}

[Serializable]
public class StartGameMessage : WebsocketMessage
{
    public List<int> players;
}

[Serializable]
public class SurrenderMessage : WebsocketMessage
{
    public bool team;
}

[Serializable]
public class GeneralPoolMessage : WebsocketMessage
{
    public List<int> generals;
}

[Serializable]
public class BanpickMessage : WebsocketMessage
{
    public int position;
    public int general;
}

[Serializable]
public class TimerMessage : WebsocketMessage
{
    public bool action;
    public List<int> cards;
    public List<int> dests;
    public string skill;
    public int src;
    public string other;
}

[Serializable]
public class PhaseMessage : WebsocketMessage
{
    public int position;
    public Phase phase;
}

[Serializable]
public class ShuffleMessage : WebsocketMessage
{
    public List<int> cards;
}

[Serializable]
public class ChangeSkinMessage : WebsocketMessage
{
    public int position;
    public int skin_id;
}

[Serializable]
public class ChatMessage : WebsocketMessage
{
    public int user_id;
    public string content;
}