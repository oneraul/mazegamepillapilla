namespace MazeGamePillaPilla
{
    class LobbyPlayer
    {
        internal string Name;
        internal string CharacterID;
        internal PlayerControllerIndex PlayerControllerIndex;

        internal LobbyPlayer(string characterID, PlayerControllerIndex playerControllerIndex)
        {
            CharacterID = characterID;
            PlayerControllerIndex = playerControllerIndex;
        }
    }
}
