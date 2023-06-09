namespace Model
{
    public class Self : GlobalSingleton<Self>
    {
        public int UserId { get; private set; }
        public string Token { get; private set; }
        public bool team { get; set; }

        public void Init(SignInResponse signInResponse)
        {
            UserId = signInResponse.user_id;
            Token = signInResponse.token;
        }
    }
}
