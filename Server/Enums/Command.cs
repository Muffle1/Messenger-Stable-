namespace Server
{
    public enum Command
    {
        Enter,
        Register,
        CreateChat,
        RemoveChat,
        AddUserToChat,
        RemoveUserFromChat,
        Message,
        AddFriend,
        RemoveFriend,
        RemoveRequest,
        GetFriend,
        GetChat,
        SendFriendRequest,
        GetMessages,
        GetFile,
        ChangeСharacteristics,
        ChangeRole,
        RemoveRole,
        AddUserToRole,
        RemoveUserFromRole,
        ChangeAccessRole,
        GetSearchedUsers,
        GetRequests,
        Notify,
        CreateServer,
        GetServer,
        ChangeServer,
        CreateChannel,
        RenameChannel,
        GetProfileInfo,
        GetChats,
        GetUser,
        GetFriends,
        JoinServer,
        AddRole,
        GetChatsByRole,
        Call,
        CallResponse,
        CallCancel,
        GetIP,
        ConnectVoiceChat,
        CheckUsersInVoiceChat,
        DeleteServer,
        RemoveUserFromServer,
        Error,
        Exit
    }
}