using NitroxModel.DataStructures.Util;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Threading
{
    public class GameActionManager
    {
        public Queue<IGameAction> gameActions = new Queue<IGameAction>();

        public void add(IGameAction gameAction)
        {
            lock (gameActions)
            {
                gameActions.Enqueue(gameAction);
            }
        }

        public Optional<IGameAction> next()
        {
            lock(gameActions)
            {
                if(gameActions.Count > 0)
                {
                    return Optional<IGameAction>.Of(gameActions.Dequeue());
                }
            }

            return Optional<IGameAction>.Empty();
        }
    }
}
