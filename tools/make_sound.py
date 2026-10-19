"""Synthesises the shouted "D K!" played when drinking Kong (own sound, no samples).

Usage: python3 make_sound.py OUT.wav

A small formant synthesiser: harmonics of a falling voice pitch are weighted by vowel
formants, consonants are shaped noise bursts, then two detuned voices, a little
saturation and a short echo make it sound like a shout.
"""
import sys
import wave

import numpy as np

RATE = 22050
rng = np.random.default_rng(7)


def formant_gain(freq, formants):
    gain = np.zeros_like(freq)
    for f, bw, amp in formants:
        gain += amp / (1.0 + ((freq - f) / (bw / 2.0)) ** 2)
    return gain


def voiced(duration, pitch_from, pitch_to, formants_from, formants_to):
    """Additive voice: every harmonic follows the pitch, its level follows the formant curve."""
    n = int(duration * RATE)
    t = np.arange(n) / RATE
    k = np.linspace(0.0, 1.0, n)
    f0 = pitch_from + (pitch_to - pitch_from) * k ** 0.7
    phase = 2 * np.pi * np.cumsum(f0) / RATE
    out = np.zeros(n)
    for h in range(1, int(4200 / min(pitch_from, pitch_to)) + 1):
        freq = h * f0
        formants = [(a[0] + (b[0] - a[0]) * k, a[1], a[2]) for a, b in zip(formants_from, formants_to)]
        gain = formant_gain(freq, formants) / h ** 0.35         # glottal tilt, bright for a shout
        gain[freq > RATE / 2 - 500] = 0
        out += gain * np.sin(h * phase + rng.uniform(0, 2 * np.pi))
    return out * (1.0 + 0.02 * np.sin(2 * np.pi * 5.5 * t))   # a little vibrato in level


def burst(duration, center, width, level):
    """Band-limited noise burst for a plosive."""
    n = int(duration * RATE)
    noise = rng.standard_normal(n)
    spectrum = np.fft.rfft(noise)
    freqs = np.fft.rfftfreq(n, 1 / RATE)
    spectrum *= np.exp(-((freqs - center) / width) ** 2)
    shaped = np.fft.irfft(spectrum, n)
    env = np.exp(-np.linspace(0, 5, n))
    return level * shaped / (np.abs(shaped).max() + 1e-9) * env


def envelope(signal, attack, release):
    n = len(signal)
    env = np.ones(n)
    a, r = int(attack * RATE), int(release * RATE)
    env[:a] = np.linspace(0, 1, a)
    env[n - r:] *= np.linspace(1, 0, r) ** 1.5
    return signal * env


# vowel formants (frequency, bandwidth, level) for a deep male shout
I = [(290, 70, 1.0), (2250, 120, 1.1), (3000, 170, 0.75), (3600, 220, 0.35)]
I_START = [(260, 70, 1.0), (1750, 120, 1.0), (2600, 170, 0.6), (3500, 220, 0.3)]      # after D
E = [(560, 90, 1.0), (1850, 130, 1.0), (2500, 180, 0.6), (3500, 220, 0.3)]


def syllable_dee(pitch):
    d = burst(0.012, 3600, 1400, 0.25)
    vowel = envelope(voiced(0.30, pitch * 1.08, pitch * 0.96, I_START, I), 0.012, 0.09)
    return np.concatenate([d, vowel])


def syllable_kay(pitch):
    closure = np.zeros(int(0.045 * RATE))
    k = burst(0.03, 2300, 900, 0.55)
    aspiration = burst(0.035, 1900, 1500, 0.18)
    vowel = envelope(voiced(0.38, pitch * 1.12, pitch * 0.78, E, I), 0.01, 0.16)
    return np.concatenate([closure, k, aspiration, vowel])


def shout(pitch):
    return np.concatenate([syllable_dee(pitch), np.zeros(int(0.09 * RATE)), syllable_kay(pitch)])


def mix():
    a = shout(118)
    b = shout(118 * 1.035)
    delay = int(0.014 * RATE)
    voice = np.zeros(max(len(a), len(b) + delay))
    voice[:len(a)] += a
    voice[delay:delay + len(b)] += 0.75 * b
    voice /= np.abs(voice).max()
    voice = np.tanh(voice * 2.2) / np.tanh(2.2)                      # shouted grit

    out = np.zeros(len(voice) + int(0.45 * RATE))
    out[:len(voice)] += voice
    for seconds, level in ((0.085, 0.28), (0.17, 0.16), (0.26, 0.08)):  # short room echo
        start = int(seconds * RATE)
        out[start:start + len(voice)] += level * voice
    out /= np.abs(out).max()
    return out * 0.89                                                 # about -1 dBFS


def main():
    samples = (mix() * 32767).astype("<i2")
    with wave.open(sys.argv[1], "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(samples.tobytes())
    print("wrote", sys.argv[1], f"{len(samples) / RATE:.2f}s")


if __name__ == "__main__":
    main()
