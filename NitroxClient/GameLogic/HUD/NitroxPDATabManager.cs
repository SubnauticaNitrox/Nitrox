using System.Collections.Generic;
#if BELOWZERO
using UnityEngine;
#endif

namespace NitroxClient.GameLogic.HUD;

public class NitroxPDATabManager
{
    public readonly Dictionary<PDATab, NitroxPDATab> CustomTabs = new();
#if SUBNAUTICA
    private readonly Dictionary<string, Atlas.Sprite> tabSpritesByName = new();
#elif BELOWZERO
    private readonly Dictionary<string, Sprite> tabSpritesByName = new();
#endif
    private readonly Dictionary<string, TabSpriteLoadedEvent> spriteLoadedCallbackByName = new();

    public NitroxPDATabManager()
    {
        void RegisterTab(NitroxPDATab nitroxTab)
        {
            CustomTabs.Add(nitroxTab.PDATabId, nitroxTab);
        }
        
        RegisterTab(new PlayerListTab());
    }
#if SUBNAUTICA
    public void AddTabSprite(string spriteName, Atlas.Sprite sprite)
#elif BELOWZERO
    public void AddTabSprite(string spriteName, Sprite sprite)
#endif
    {
        tabSpritesByName.Add(spriteName, sprite);
        if (spriteLoadedCallbackByName.TryGetValue(spriteName, out TabSpriteLoadedEvent spriteLoadedEvent))
        {
            spriteLoadedEvent.Invoke(sprite);
            spriteLoadedCallbackByName.Remove(spriteName);
        }
    }
#if SUBNAUTICA
    public bool TryGetTabSprite(string spriteName, out Atlas.Sprite sprite) => tabSpritesByName.TryGetValue(spriteName, out sprite);

    public delegate void TabSpriteLoadedEvent(Atlas.Sprite sprite);
#elif BELOWZERO
    public bool TryGetTabSprite(string spriteName, out Sprite sprite) => tabSpritesByName.TryGetValue(spriteName, out sprite);

    public delegate void TabSpriteLoadedEvent(Sprite sprite);
#endif

    public void SetSpriteLoadedCallback(string tabName, TabSpriteLoadedEvent callback)
    {
        if (!spriteLoadedCallbackByName.ContainsKey(tabName))
        {
            spriteLoadedCallbackByName.Add(tabName, callback);
        }
    }
}
