using System.Collections.Generic;

namespace NitroxClient.GameLogic.HUD;

public class NitroxPDATabManager
{
    public readonly Dictionary<PDATab, NitroxPDATab> CustomTabs = new();
    
    private readonly Dictionary<string, Atlas.Sprite> tabSpritesByName = new();
    private readonly Dictionary<string, TabSpriteLoadedEvent> spriteLoadedCallbackByName = new();

    public NitroxPDATabManager()
    {
        void RegisterTab(NitroxPDATab nitroxTab)
        {
            CustomTabs.Add(nitroxTab.PDATabId, nitroxTab);
        }
        
        RegisterTab(new PlayerListTab());
    }

    public void AddTabSprite(string spriteName, Atlas.Sprite sprite)
    {
        tabSpritesByName.Add(spriteName, sprite);
        if (spriteLoadedCallbackByName.TryGetValue(spriteName, out TabSpriteLoadedEvent spriteLoadedEvent))
        {
            spriteLoadedEvent.Invoke(sprite);
            spriteLoadedCallbackByName.Remove(spriteName);
        }
    }
    
    public bool TryGetTabSprite(string spriteName, out Atlas.Sprite sprite) => tabSpritesByName.TryGetValue(spriteName, out sprite);

    public delegate void TabSpriteLoadedEvent(Atlas.Sprite sprite);
    
    public void SetSpriteLoadedCallback(string tabName, TabSpriteLoadedEvent callback)
    {
        if (!spriteLoadedCallbackByName.ContainsKey(tabName))
        {
            spriteLoadedCallbackByName.Add(tabName, callback);
        }
    }
}
