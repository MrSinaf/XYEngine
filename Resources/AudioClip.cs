﻿using NVorbis;
using Silk.NET.OpenAL;
using static XYEngine.Audio;

namespace XYEngine.Resources;

public class AudioClip : IAsset
{
	private uint handle;
	private uint source;
	
	private void LoadWav(Stream stream)
	{
		using var reader = new BinaryReader(stream);
		
		if (new string(reader.ReadChars(4)) != "RIFF")
			throw new FormatException("Invalid WAV file. Missing 'RIFF' header.");
		
		reader.ReadInt32();
		
		if (new string(reader.ReadChars(4)) != "WAVE")
			throw new FormatException("Invalid WAV file. Missing 'WAVE' format identifier.");
		
		short numChannels = -1;
		var sampleRate = -1;
		short bitsPerSample = -1;
		byte[] audioData = null;
		
		while (reader.BaseStream.Position < reader.BaseStream.Length)
		{
			var chunkId = new string(reader.ReadChars(4));
			var chunkSize = reader.ReadInt32();
			
			switch (chunkId)
			{
				case "fmt ":
					var audioFormat = reader.ReadInt16();
					if (audioFormat != 1)
						throw new FormatException($"Unsupported WAV format: {audioFormat} (only PCM is supported).");
					
					numChannels = reader.ReadInt16();
					sampleRate = reader.ReadInt32();
					reader.ReadInt32();
					reader.ReadInt16();
					bitsPerSample = reader.ReadInt16();
					break;
				
				case "data":
					audioData = reader.ReadBytes(chunkSize);
					break;
				
				default:
					reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
					break;
			}
		}
		
		if (audioData == null)
			throw new FormatException("Invalid WAV file. Missing 'data' chunk.");
		
		if (numChannels == -1 || sampleRate == -1 || bitsPerSample == -1)
			throw new FormatException("Invalid WAV file. Missing 'fmt ' chunk.");
		
		if (bitsPerSample != 8 && bitsPerSample != 16)
			throw new FormatException("Unsupported bits per sample. Only 8 or 16 bits are supported.");
		
		var format = numChannels switch
		{
			1 => bitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16,
			2 => bitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16,
			_ => throw new FormatException($"Unsupported channel count: {numChannels}.")
		};
		
		if (audioData.Length % (bitsPerSample / 8) != 0)
			throw new FormatException("Invalid audio data length.");
		
		handle = CreateBuffer(audioData, format, sampleRate);
		source = CreateSource(handle);
	}
	
	private void LoadOgg(Stream stream)
	{
		using var vorbis = new VorbisReader(stream);
		using var pcmStream = new MemoryStream();
		
		var buffer = new float[4096];
		int bytesRead;
		while ((bytesRead = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
		{
			var pcmBuffer = new short[bytesRead];
			for (var i = 0; i < bytesRead; i++)
				pcmBuffer[i] = (short)(buffer[i] * short.MaxValue);
			
			var byteArray = new byte[pcmBuffer.Length * sizeof(short)];
			Buffer.BlockCopy(pcmBuffer, 0, byteArray, 0, byteArray.Length);
			pcmStream.Write(byteArray, 0, byteArray.Length);
		}
		
		handle = CreateBuffer(pcmStream.ToArray(), vorbis.Channels == 1 ? BufferFormat.Mono16 : BufferFormat.Stereo16, vorbis.SampleRate);
		source = CreateSource(handle);
	}
	
	public void Play() => al.SourcePlay(source);
	
	public void Pause() => al.SourcePause(source);
	
	public void Stop() => al.SourceStop(source);
	
	void IAsset.Load(AssetProperty property)
	{
		var stream = property.stream;
		switch (property.extension)
		{
			case ".wav":
				LoadWav(stream);
				break;
			case ".ogg":
				LoadOgg(stream);
				break;
			default:
				throw new NotSupportedException($"Unsupported audio format: {property.extension}.Expected '.ogg' or '.wav'");
		}
	}
	
	void IAsset.UnLoad()
	{
		al.DeleteBuffer(handle);
		al.DeleteSource(source);
	}
}