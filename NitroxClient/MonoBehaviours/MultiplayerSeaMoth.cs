namespace NitroxClient.MonoBehaviours
{
    class MultiplayerSeaMoth : MultiplayerVehicleControl<Vehicle>
    {
        private bool lastThrottle = false;
        private SeaMoth seamoth;

        protected override void Start()
        {
            steeringControl = seamoth = GetComponent<SeaMoth>();
            base.Start();
        }

        internal override void SetThrottle(bool isOn)
        {
            if (isOn != lastThrottle)
            {
                if (isOn)
                {
                    seamoth.bubbles.Play();
                }
                else
                {
                    seamoth.bubbles.Stop();
                }

                lastThrottle = isOn;
            }
        }
    }
}
