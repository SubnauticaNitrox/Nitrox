using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020008D7 RID: 2263
public class uGUI_Binding_Nitrox : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler, ICancelHandler
{
    // Token: 0x06003BA6 RID: 15270 RVA: 0x0002808B File Offset: 0x0002628B
    protected override void OnEnable()
    {
        base.OnEnable();
        GameInput.OnPrimaryDeviceChanged += this.OnPrimaryDeviceChanged;
        //GameInput.OnBindingsChanged += this.OnBindingsChanged;
        this.UpdateHighlightEffect();
    }

    // Token: 0x06003BA7 RID: 15271 RVA: 0x000280BB File Offset: 0x000262BB
    protected override void OnDisable()
    {
        base.OnDisable();
        GameInput.OnPrimaryDeviceChanged -= this.OnPrimaryDeviceChanged;
        //GameInput.OnBindingsChanged -= this.OnBindingsChanged;
    }

    // Token: 0x170003DD RID: 989
    // (get) Token: 0x06003BA8 RID: 15272 RVA: 0x000280E5 File Offset: 0x000262E5
    // (set) Token: 0x06003BA9 RID: 15273 RVA: 0x000280ED File Offset: 0x000262ED
    public string value
    {
        get {
            return this.currentValue;
        }
        set {
            if (Application.isPlaying && this.currentValue == value)
            {
                return;
            }
            this.currentValue = value;
            this.RefreshShownValue();
            this.valueChanged.Invoke(this.currentValue);
        }
    }

    // Token: 0x170003DE RID: 990
    // (get) Token: 0x06003BAA RID: 15274 RVA: 0x00028129 File Offset: 0x00026329
    // (set) Token: 0x06003BAB RID: 15275 RVA: 0x00028131 File Offset: 0x00026331
    public uGUI_Binding.BindingEvent onValueChanged
    {
        get {
            return this.valueChanged;
        }
        set {
            this.valueChanged = value;
        }
    }

    // Token: 0x06003BAC RID: 15276 RVA: 0x0002813A File Offset: 0x0002633A
    protected override void Start()
    {
        base.Start();
        this.RefreshShownValue();
    }

    // Token: 0x06003BAD RID: 15277 RVA: 0x00166EE4 File Offset: 0x001650E4
    private void Update()
    {
        if (this.active)
        {
            string pressedInput = GameInput.GetPressedInput(this.device);
            if (pressedInput != null)
            {
                this.SetActive(false);
                this.value = pressedInput;
                GameInput.ClearInput();
            }
        }
        else if (base.gameObject == EventSystem.current.currentSelectedGameObject && GameInput.GetButtonDown(GameInput.Button.UIClear))
        {
            this.value = null;
        }
    }

    // Token: 0x06003BAE RID: 15278 RVA: 0x00028148 File Offset: 0x00026348
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        this.SetActive(!this.active);
    }

    // Token: 0x06003BAF RID: 15279 RVA: 0x00028159 File Offset: 0x00026359
    public virtual void OnSubmit(BaseEventData eventData)
    {
        this.SetActive(true);
    }

    // Token: 0x06003BB0 RID: 15280 RVA: 0x00028162 File Offset: 0x00026362
    public virtual void OnCancel(BaseEventData eventData)
    {
        this.SetActive(false);
    }

    // Token: 0x06003BB1 RID: 15281 RVA: 0x0002816B File Offset: 0x0002636B
    private void SetActive(bool _active)
    {
        this.active = _active;
        this.RefreshShownValue();
    }

    // Token: 0x06003BB2 RID: 15282 RVA: 0x00166F54 File Offset: 0x00165154
    private void RefreshShownValue()
    {
        if (this.active || this.currentValue == null)
        {
            this.currentText.text = string.Empty;
        }
        else
        {
            this.currentText.text = currentValue;
        }
    }

    // Token: 0x06003BB3 RID: 15283 RVA: 0x0002817A File Offset: 0x0002637A
    private void OnPrimaryDeviceChanged()
    {
        this.UpdateHighlightEffect();
    }

    // Token: 0x06003BB4 RID: 15284 RVA: 0x00028182 File Offset: 0x00026382
    private void OnBindingsChanged()
    {
        this.RefreshShownValue();
    }

    // Token: 0x06003BB5 RID: 15285 RVA: 0x0002818A File Offset: 0x0002638A
    private void UpdateHighlightEffect()
    {
        if (GameInput.GetPrimaryDevice() == GameInput.Device.Controller)
        {
            base.transition = Selectable.Transition.SpriteSwap;
        }
        else
        {
            base.transition = Selectable.Transition.None;
            base.image.overrideSprite = null;
        }
    }

    // Token: 0x04003A18 RID: 14872
    private bool active;

    // Token: 0x04003A19 RID: 14873
    private string currentValue;

    // Token: 0x04003A1A RID: 14874
    public GameInput.Device device;

    // Token: 0x04003A1B RID: 14875
    public Text currentText;

    // Token: 0x04003A1C RID: 14876
    private uGUI_Binding.BindingEvent valueChanged = new uGUI_Binding.BindingEvent();

    // Token: 0x020008D8 RID: 2264 
    public class BindingEvent : UnityEvent<string>
    {
    }
}
