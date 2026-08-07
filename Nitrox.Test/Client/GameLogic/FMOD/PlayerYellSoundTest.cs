using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic.FMOD;

namespace Nitrox.Test.Client.GameLogic.FMOD;

[TestClass]
public sealed class PlayerYellSoundTest
{
    [TestMethod]
    public void BundledAudioBankMatchesPacketAndUsesMonoPcmWave()
    {
        PlayerYellSound.AudioFileNames.Should().HaveCount(PlayerYell.SOUND_COUNT);
        PlayerYellSound.AudioFileNames.Should().OnlyHaveUniqueItems();

        for (int soundIndex = 0; soundIndex < PlayerYell.SOUND_COUNT; soundIndex++)
        {
            string audioFilePath = PlayerYellSound.GetAudioFilePath(soundIndex);
            File.Exists(audioFilePath).Should().BeTrue($"yell sound {soundIndex} should exist at '{audioFilePath}'");

            using FileStream stream = File.OpenRead(audioFilePath);
            using BinaryReader reader = new(stream);

            new string(reader.ReadChars(4)).Should().Be("RIFF");
            reader.ReadUInt32();
            new string(reader.ReadChars(4)).Should().Be("WAVE");

            WaveFormat format = ReadWaveFormat(reader);
            format.Encoding.Should().Be(1, "the runtime loader expects uncompressed PCM");
            format.Channels.Should().Be(1, "a point-source 3D sound must be mono");
            format.SampleRate.Should().Be(48000);
            format.BitsPerSample.Should().Be(16);
        }
    }

    private static WaveFormat ReadWaveFormat(BinaryReader reader)
    {
        while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
        {
            string chunkId = new(reader.ReadChars(4));
            uint chunkLength = reader.ReadUInt32();
            if (chunkId == "fmt ")
            {
                ushort encoding = reader.ReadUInt16();
                ushort channels = reader.ReadUInt16();
                uint sampleRate = reader.ReadUInt32();
                reader.BaseStream.Seek(6, SeekOrigin.Current);
                ushort bitsPerSample = reader.ReadUInt16();
                return new WaveFormat(encoding, channels, sampleRate, bitsPerSample);
            }

            reader.BaseStream.Seek(chunkLength + (chunkLength & 1), SeekOrigin.Current);
        }

        Assert.Fail("The bundled yell WAV has no format chunk");
        return default;
    }

    private readonly record struct WaveFormat(ushort Encoding, ushort Channels, uint SampleRate, ushort BitsPerSample);
}
