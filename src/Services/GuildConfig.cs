namespace FrenBot.Services
{
    public class GuildConfig
    {
        public bool NotifyEnabled { get; set; }

        public ulong NotifyChannelID { get; set; }
        public ulong NotifyRoleID { get; set; }
    }
}
