﻿using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

static public class shaco_ExtensionsAudioClip
{
	private  const int HEADER_SIZE = 44;
	private struct ClipData
	{
		public int samples;
		public int channels;
		public float[] samplesData;

	}

	/// <summary>
	/// 获取音频对象二进制信息
	/// <param name="clip">音频对象</param>
	/// <return>二进制信息，如果转换失败返回null</return>
	/// </summary>
	static public byte[] EncodeToWav(this AudioClip clip)
	{
		var tmpPath = Application.persistentDataPath.ContactPath("tmp.wav");
		SaveAsFile(clip, tmpPath);
		var retValue = shaco.Base.FileHelper.ReadAllByteByUserPath(tmpPath);
		shaco.Base.FileHelper.DeleteByUserPath(tmpPath);
		return retValue;
	}

	/// <summary>
	/// 保存音频为wav文件
	/// <param name="clip">音频对象</param>
	/// <param name="path">保存文件路径，只支持wav格式</param>
	/// <return>是否保存成功</return>
	/// </summary>
	static public bool SaveAsFile(this AudioClip clip, string path)
	{
		if (!path.ToLower().EndsWith(".wav"))
		{
			path += ".wav";
		}

		var filepath = path;

		Debug.Log(filepath);

		// Make sure directory exists if user is saving to sub dir.
		Directory.CreateDirectory(Path.GetDirectoryName(filepath));
		ClipData clipdata = new ClipData();
		clipdata.samples = clip.samples;
		clipdata.channels = clip.channels;
		float[] dataFloat = new float[clip.samples * clip.channels];
		clip.GetData(dataFloat, 0);
		clipdata.samplesData = dataFloat;
		using (var fileStream = CreateEmpty(filepath))
		{
			MemoryStream memstrm = new MemoryStream();
			ConvertAndWrite(memstrm, clipdata);
			memstrm.WriteTo(fileStream);
			WriteHeader(fileStream, clip);
		}

		return true; // TODO: return false if there's a failure saving the file
	}

	static public AudioClip TrimSilence(this AudioClip clip, float min)
	{
		var samples = new float[clip.samples];

		clip.GetData(samples, 0);

		return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
	}

	static private AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
	{
		return TrimSilence(samples, min, channels, hz, false);
	}

	static private AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool stream)
	{
		int i;

		for (i = 0; i < samples.Count; i++)
		{
			if (Mathf.Abs(samples[i]) > min)
			{
				break;
			}
		}

		samples.RemoveRange(0, i);

		for (i = samples.Count - 1; i > 0; i--)
		{
			if (Mathf.Abs(samples[i]) > min)
			{
				break;
			}
		}

		samples.RemoveRange(i, samples.Count - i);

		var clip = AudioClip.Create("TempClip", samples.Count, channels, hz, stream);

		clip.SetData(samples.ToArray(), 0);

		return clip;
	}

	static private FileStream CreateEmpty(string filepath)
	{
		var fileStream = new FileStream(filepath, FileMode.Create);
		byte emptyByte = new byte();

		for (int i = 0; i < HEADER_SIZE; i++) //preparing the header
		{
			fileStream.WriteByte(emptyByte);
		}

		return fileStream;
	}

	static private void ConvertAndWrite(MemoryStream memStream, ClipData clipData)
	{
		float[] samples = new float[clipData.samples * clipData.channels];

		samples = clipData.samplesData;

		Int16[] intData = new Int16[samples.Length];

		Byte[] bytesData = new Byte[samples.Length * 2];

		const float rescaleFactor = 32767; //to convert float to Int16

		for (int i = 0; i < samples.Length; i++)
		{
			intData[i] = (short)(samples[i] * rescaleFactor);
			//Debug.Log (samples [i]);
		}
		Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
		memStream.Write(bytesData, 0, bytesData.Length);
	}

	static private void WriteHeader(FileStream fileStream, AudioClip clip)
	{

		var hz = clip.frequency;
		var channels = clip.channels;
		var samples = clip.samples;

		fileStream.Seek(0, SeekOrigin.Begin);

		Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(riff, 0, 4);

		Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
		fileStream.Write(chunkSize, 0, 4);

		Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(wave, 0, 4);

		Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(fmt, 0, 4);

		Byte[] subChunk1 = BitConverter.GetBytes(16);
		fileStream.Write(subChunk1, 0, 4);

		// UInt16 two = 2;
		UInt16 one = 1;

		Byte[] audioFormat = BitConverter.GetBytes(one);
		fileStream.Write(audioFormat, 0, 2);

		Byte[] numChannels = BitConverter.GetBytes(channels);
		fileStream.Write(numChannels, 0, 2);

		Byte[] sampleRate = BitConverter.GetBytes(hz);
		fileStream.Write(sampleRate, 0, 4);

		Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
		fileStream.Write(byteRate, 0, 4);

		UInt16 blockAlign = (ushort)(channels * 2);
		fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

		UInt16 bps = 16;
		Byte[] bitsPerSample = BitConverter.GetBytes(bps);
		fileStream.Write(bitsPerSample, 0, 2);

		Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
		fileStream.Write(datastring, 0, 4);

		Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
		fileStream.Write(subChunk2, 0, 4);

		//        fileStream.Close();
	}
}