/* AU Assets Swapper - Made by Isam Ahmed (isam-ahme0, isam8087, isam0) - 2026
 * 
 * This file is part of AU Assets Swapper.
 * 
 * AU Assets Swapper is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * AU Assets Swapper is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with AU Assets Swapper. If not, see <https://www.gnu.org/licenses/>.
 * https://gamebanana.com/mods/719417
 */
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace AU_Assets_Swapper;

internal static class AudioLoader
{
    public static AudioClip LoadAudioClip(string filePath, string clipName)
    {
        if (!File.Exists(filePath)) return null;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var data = File.ReadAllBytes(filePath);

        switch (ext)
        {
            case ".wav": return LoadWav(data, clipName);
            case ".ogg": return LoadOgg(data, clipName);
            default:
                Plugin.LogSource.LogWarning($"[AUAS] unsupported audio format: {ext}. use WAV or OGG.");
                return null;
        }
    }

    private static AudioClip LoadWav(byte[] data, string clipName)
    {
        try
        {
            if (data.Length < 44) return null;

            if (System.Text.Encoding.ASCII.GetString(data, 0, 4) != "RIFF" ||
                System.Text.Encoding.ASCII.GetString(data, 8, 4) != "WAVE")
            {
                Plugin.LogSource.LogWarning($"[AUAS] bad WAV: {clipName}");
                return null;
            }

            int channels = BitConverter.ToInt16(data, 22);
            int rate = BitConverter.ToInt32(data, 24);
            int bps = BitConverter.ToInt16(data, 34);

            if (channels <= 0 || rate <= 0 || bps <= 0) return null;

            int bytesPerSample = bps / 8;

            // find the "data" chunk (some wav files have extra chunks before it)
            int offset = 44;
            while (offset < data.Length - 8)
            {
                string id = System.Text.Encoding.ASCII.GetString(data, offset, 4);
                int sz = BitConverter.ToInt32(data, offset + 4);
                if (id == "data") break;
                offset += 8 + sz;
            }

            if (offset >= data.Length) return null;

            int dataSize = BitConverter.ToInt32(data, offset + 4);
            int sampleCount = dataSize / (channels * bytesPerSample);
            offset += 8;

            var samples = new float[sampleCount * channels];
            for (int i = 0; i < samples.Length; i++)
            {
                int byteIdx = offset + i * bytesPerSample;
                if (byteIdx + bytesPerSample > data.Length) break;

                if (bps == 16)
                    samples[i] = BitConverter.ToInt16(data, byteIdx) / 32768f;
                else if (bps == 8)
                    samples[i] = (data[byteIdx] - 128) / 128f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, channels, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] WAV parse error: {ex.Message}");
            return null;
        }
    }

    // hacky but works - dump to temp file and use unity'sWebRequest to decode
    private static AudioClip LoadOgg(byte[] data, string clipName)
    {
        var tmp = Path.Combine(Path.GetTempPath(), "auas_" + Guid.NewGuid().ToString("N") + ".ogg");
        try
        {
            File.WriteAllBytes(tmp, data);
            var uri = "file:///" + tmp.Replace("\\", "/");
            var www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS);
            var op = www.SendWebRequest();

            // spin until done, with a timeout so we don't hang forever
            var deadline = Environment.TickCount + 10000;
            while (!op.isDone)
            {
                if (Environment.TickCount > deadline)
                {
                    Plugin.LogSource.LogWarning($"[AUAS] OGG load timed out: {clipName}");
                    www.Abort();
                    return null;
                }
                Thread.Sleep(16);
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Plugin.LogSource.LogWarning($"[AUAS] OGG load failed: {www.error}");
                return null;
            }

            var clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip != null) clip.name = clipName;
            return clip;
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] OGG error: {ex.Message}");
            return null;
        }
        finally
        {
            try { File.Delete(tmp); } catch { }
        }
    }
}
