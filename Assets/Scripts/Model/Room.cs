using System.Collections.Generic;

namespace Model
{
    public class User
    {
        public int id;
        public int position;
        public bool already;

        public const int StandaloneId = 0;
        public const int AIId = -1;
    }

    public class JoinRoom : Message
    {
        public string mode;
        public List<User> users;
        public int ownerId;
    }

    public class AddUser : Message
    {
        public User user;
    }

    public class RemoveUser : Message
    {
        public int userId;
        public int ownerId;
    }

    public class SetAlready : Message
    {
        public int userId;
        public bool value;
    }

    public class StartGame : Message { }
}
