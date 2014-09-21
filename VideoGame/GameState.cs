using Microsoft.Xna.Framework;

namespace VideoGame
{
    enum State
    {
        menu,
        play,
        over,
    }
    class GameState
    {
        public State state = State.menu;
    }
}
