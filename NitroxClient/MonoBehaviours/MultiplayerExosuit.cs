using NitroxModel.Helper;

namespace NitroxClient.MonoBehaviours
{
    class MultiplayerExosuit : MultiplayerVehicleControl<Vehicle>
    {
        private bool lastThrottle = false;
        private Exosuit exosuit;

        protected override void Awake()
        {
            steeringControl = exosuit = GetComponent<Exosuit>();
            base.Awake();
        }

        internal override void SetThrottle(bool isOn)
        {
            // IDEA: ReflectionHelper methods for PropertyInfo, instead of using the raw methods?
            exosuit.ReflectionCall("set_jetsActive", false, false, new object[] { isOn });
            //if (isOn)
            //{
            //    exosuit.loopingJetSound.Play();
            //    exosuit.fxcontrol.Play(0);
            //}
            //else
            //{
            //    exosuit.loopingJetSound.Stop();
            //    exosuit.fxcontrol.Stop(0);
            //}
        }
    }
}
