namespace MazeGamePillaPilla
{
    enum NetMessage
    {
        // shared
        ServerClosed,
        ClientClosed,

        // lobby
        ClientsCount,
        AddPlayer,
        RemovePlayer,
        PrepareToStartGame,
        ReadyToStart,
        StartGame,
        InstantiateCharacter,

        // gameplay
        CharacterUpdate,
        GoToScoresScreen,
    }
}
