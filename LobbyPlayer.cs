namespace MazeGamePillaPilla
{
    class LobbyPlayer
    {
        public string Name;
        public string CharacterID;
        public PlayerControllerIndex PlayerControllerIndex;

        public LobbyPlayer(string characterID, PlayerControllerIndex playerControllerIndex)
        {
            CharacterID = characterID;
            PlayerControllerIndex = playerControllerIndex;
        }
    }
}
