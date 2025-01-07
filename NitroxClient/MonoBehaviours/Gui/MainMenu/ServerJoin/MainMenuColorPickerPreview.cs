using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;

public class MainMenuColorPickerPreview : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Image previewImage;
    private CanvasGroup cg;

    public void Init(uGUI_ColorPicker colorPicker)
    {
        GameObject colorPreview = new("ColorPreview");
        colorPreview.transform.SetParent(colorPicker.pointer.transform);
        colorPreview.transform.localPosition = new Vector3(-30, 30, 0);
        colorPreview.transform.localRotation = Quaternion.identity;
        colorPreview.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        previewImage = colorPreview.AddComponent<Image>();
        previewImage.sprite = CreateCircleSprite();
        cg = colorPreview.AddComponent<CanvasGroup>();
        cg.alpha = 0;

        colorPicker.onColorChange.AddListener(OnColorPickerDrag);
    }

    private static Sprite CreateCircleSprite()
    {
        const int HALF_SIZE = 50;
        const int RADIUS = 42;
        Texture2D tex = new(HALF_SIZE * 2, HALF_SIZE * 2);
        for (int y = -HALF_SIZE; y <= HALF_SIZE; y++)
        {
            for (int x = -HALF_SIZE; x <= HALF_SIZE; x++)
            {
                bool isInsideCircle = x * x + y * y <= RADIUS * RADIUS;
                tex.SetPixel(HALF_SIZE + x, HALF_SIZE + y, isInsideCircle ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 200);
    }

    private void OnColorPickerDrag(ColorChangeEventData data) => previewImage.color = data.color;

    public void OnPointerDown(PointerEventData _)
    {
        StopAllCoroutines();
        StartCoroutine(cg.ShiftAlpha(1, 0.25f, 1.5f, true));
    }

    public void OnPointerUp(PointerEventData _)
    {
        StopAllCoroutines();
        StartCoroutine(cg.ShiftAlpha(0, 0.25f, 1.5f, false));
    }
}
