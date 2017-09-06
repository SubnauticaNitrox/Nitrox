namespace NitroxClient.MonoBehaviours
{
    class MultiplayerSeaMoth : MultiplayerVehicleControl<Vehicle>
    {
        private bool lastThrottle = false;
        private SeaMoth seamoth;

        private void Start()
        {
            steeringControl = seamoth = GetComponent<SeaMoth>();
        }

        public void SetThrottle(bool isOn)
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
