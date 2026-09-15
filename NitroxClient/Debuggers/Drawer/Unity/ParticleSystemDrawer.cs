using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public sealed class ParticleSystemDrawer : IDrawer<ParticleSystem>
{
    public void Draw(ParticleSystem particleSystem)
    {
        ParticleSystem.MainModule main = particleSystem.main;
        ParticleSystem.EmissionModule emission = particleSystem.emission;

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Duration", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            main.duration = NitroxGUILayout.FloatField(main.duration, NitroxGUILayout.VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Loop", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            main.loop = NitroxGUILayout.BoolField(main.loop, NitroxGUILayout.VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Play On Awake", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            main.playOnAwake = NitroxGUILayout.BoolField(main.playOnAwake, NitroxGUILayout.VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Start Lifetime", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            main.startLifetime = new ParticleSystem.MinMaxCurve(NitroxGUILayout.FloatField(main.startLifetime.constant, NitroxGUILayout.VALUE_WIDTH));
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Start Speed", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            main.startSpeed = new ParticleSystem.MinMaxCurve(NitroxGUILayout.FloatField(main.startSpeed.constant, NitroxGUILayout.VALUE_WIDTH));
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Max Particles", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            main.maxParticles = Mathf.Max(0, NitroxGUILayout.IntField(main.maxParticles, NitroxGUILayout.VALUE_WIDTH));
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Simulation Space", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            main.simulationSpace = NitroxGUILayout.EnumPopup(main.simulationSpace, NitroxGUILayout.VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Emission", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            emission.enabled = NitroxGUILayout.BoolField(emission.enabled, NitroxGUILayout.VALUE_WIDTH);
        }
    }
}
