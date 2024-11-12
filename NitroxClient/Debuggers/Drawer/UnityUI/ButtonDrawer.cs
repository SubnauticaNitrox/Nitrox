using NitroxClient.Debuggers.Drawer.Unity;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ButtonDrawer : IDrawer<Button>
{
    private readonly SelectableDrawer selectableDrawer;
    private readonly UnityEventDrawer unityEventDrawer;

    public ButtonDrawer(SelectableDrawer selectableDrawer, UnityEventDrawer unityEventDrawer)
    {
        Validate.NotNull(selectableDrawer);
        Validate.NotNull(unityEventDrawer);
        this.selectableDrawer = selectableDrawer;
        this.unityEventDrawer = unityEventDrawer;
    }

    public void Draw(Button button)
    {
        selectableDrawer.Draw(button);
        GUILayout.Space(10);
        unityEventDrawer.Draw(button.onClick, new UnityEventDrawer.DrawOptions("OnClick()"));
    }
}
