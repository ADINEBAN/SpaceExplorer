using UnityEngine;

public static class SpaceGameAudio
{
    private const int SampleRate = 22050;

    private static AudioClip laserShotClip;
    private static AudioClip asteroidDestroyedClip;
    private static AudioClip playerDestroyedClip;
    private static AudioClip starCollectedClip;
    private static AudioClip victoryFanfareClip;

    public static void PlayVictoryFanfare(Vector3 position, float volume = 1f)
    {
        PlayDetached(
            position,
            GetClip(ref victoryFanfareClip, SoundType.VictoryFanfare),
            volume
        );
    }

    public static void PlayLaserShot(AudioSource source, float volume)
    {
        if (source != null)
        {
            source.PlayOneShot(
                GetClip(ref laserShotClip, SoundType.LaserShot),
                volume
            );
        }
    }

    public static void PlayAsteroidDestroyed(Vector3 position, float volume)
    {
        PlayDetached(
            position,
            GetClip(ref asteroidDestroyedClip, SoundType.AsteroidDestroyed),
            volume
        );
    }

    public static void PlayPlayerDestroyed(Vector3 position, float volume)
    {
        PlayDetached(
            position,
            GetClip(ref playerDestroyedClip, SoundType.PlayerDestroyed),
            volume
        );
    }

    public static void PlayStarCollected(Vector3 position, float volume)
    {
        PlayDetached(
            position,
            GetClip(ref starCollectedClip, SoundType.StarCollected),
            volume
        );
    }

    private static void PlayDetached(
        Vector3 position,
        AudioClip clip,
        float volume
    )
    {
        GameObject audioObject = new GameObject("SpaceGameAudio");
        audioObject.transform.position = position;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = volume;
        source.clip = clip;
        source.Play();

        Object.Destroy(audioObject, clip.length + 0.1f);
    }

    private static AudioClip GetClip(ref AudioClip clip, SoundType soundType)
    {
        if (clip == null)
        {
            clip = CreateClip(soundType);
        }

        return clip;
    }

    private static AudioClip CreateClip(SoundType soundType)
    {
        float duration = soundType switch
        {
            SoundType.LaserShot => 0.12f,
            SoundType.AsteroidDestroyed => 0.34f,
            SoundType.PlayerDestroyed => 0.62f,
            SoundType.VictoryFanfare => 1.4f,
            _ => 0.24f
        };

        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float phase = 0f;
        uint noiseState = (uint)(soundType + 1) * 747796405u;

        for (int i = 0; i < sampleCount; i++)
        {
            float progress = i / (float)sampleCount;
            float sample = soundType switch
            {
                SoundType.LaserShot => CreateLaserSample(progress, ref phase),
                SoundType.AsteroidDestroyed =>
                    CreateAsteroidSample(progress, ref phase, ref noiseState),
                SoundType.PlayerDestroyed =>
                    CreatePlayerDestroyedSample(progress, ref phase, ref noiseState),
                SoundType.VictoryFanfare =>
                    CreateVictoryFanfareSample(progress, ref phase),
                _ => CreateStarSample(progress, ref phase)
            };

            samples[i] = sample * 0.75f;
        }

        AudioClip clip = AudioClip.Create(
            soundType.ToString(),
            sampleCount,
            1,
            SampleRate,
            false
        );
        clip.SetData(samples, 0);
        return clip;
    }

    private static float CreateLaserSample(float progress, ref float phase)
    {
        float frequency = Mathf.Lerp(1350f, 420f, progress);
        phase += 2f * Mathf.PI * frequency / SampleRate;
        float envelope = (1f - progress) * (1f - progress);
        return Mathf.Sin(phase) * envelope;
    }

    private static float CreateAsteroidSample(
        float progress,
        ref float phase,
        ref uint noiseState
    )
    {
        float frequency = Mathf.Lerp(180f, 48f, progress);
        phase += 2f * Mathf.PI * frequency / SampleRate;
        float envelope = (1f - progress) * (1f - progress);
        float noise = NextNoise(ref noiseState);
        return (Mathf.Sin(phase) * 0.7f + noise * 0.3f) * envelope;
    }

    private static float CreatePlayerDestroyedSample(
        float progress,
        ref float phase,
        ref uint noiseState
    )
    {
        float frequency = Mathf.Lerp(115f, 32f, progress);
        phase += 2f * Mathf.PI * frequency / SampleRate;
        float envelope = Mathf.Exp(-5.5f * progress);
        float noise = NextNoise(ref noiseState);
        return (Mathf.Sin(phase) * 0.65f + noise * 0.5f) * envelope;
    }

    private static float CreateStarSample(float progress, ref float phase)
    {
        float frequency = Mathf.Lerp(720f, 1280f, progress);
        phase += 2f * Mathf.PI * frequency / SampleRate;
        float envelope = Mathf.Sin(Mathf.PI * progress) *
            Mathf.Exp(-2.2f * progress);
        return (
            Mathf.Sin(phase) * 0.7f +
            Mathf.Sin(phase * 1.5f) * 0.3f
        ) * envelope;
    }

    private static float CreateVictoryFanfareSample(float progress, ref float phase)
    {
        float freq;
        float noteProgress;
        if (progress < 0.22f)
        {
            freq = 523.25f; // C5
            noteProgress = progress / 0.22f;
        }
        else if (progress < 0.44f)
        {
            freq = 659.25f; // E5
            noteProgress = (progress - 0.22f) / 0.22f;
        }
        else if (progress < 0.66f)
        {
            freq = 783.99f; // G5
            noteProgress = (progress - 0.44f) / 0.22f;
        }
        else
        {
            freq = 1046.50f; // C6 (High triumph)
            noteProgress = (progress - 0.66f) / 0.34f;
        }

        phase += 2f * Mathf.PI * freq / SampleRate;
        float env = Mathf.Exp(-3.0f * noteProgress) * Mathf.Sin(Mathf.Clamp01(noteProgress * 10f) * Mathf.PI * 0.5f);
        return (Mathf.Sin(phase) * 0.62f + Mathf.Sin(phase * 2f) * 0.26f + Mathf.Sin(phase * 3f) * 0.12f) * env;
    }

    private static float NextNoise(ref uint state)
    {
        state = state * 1664525u + 1013904223u;
        return ((state >> 8) / 8388607.5f) - 1f;
    }

    private enum SoundType
    {
        LaserShot,
        AsteroidDestroyed,
        PlayerDestroyed,
        StarCollected,
        VictoryFanfare
    }
}
