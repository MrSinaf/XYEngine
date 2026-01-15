using NAudio.Wave;
using NVorbis;
using Silk.NET.OpenAL;
using XYEngine.Debugs;
using XYEngine.Utils;
using static XYEngine.Audio;

namespace XYEngine.Resources;

public class AudioClip : IResource, IDebugProperty
{
	private uint source;
	
	public string assetPath { get; set; }
	public uint buffer { get; private set; }
	public int duration { get; private set; }
	
	public float currentTime
	{
		get
		{
			al.GetSourceProperty(source, SourceFloat.SecOffset, out var time);
			return time;
		}
		set => al.SetSourceProperty(source, SourceFloat.SecOffset, MathUtils.Range(value, 0, duration));
	}
	
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
		
		duration = audioData.Length / (sampleRate * numChannels * (bitsPerSample / 8));
		buffer = CreateBuffer(audioData, format, sampleRate);
		source = CreateSource(buffer);
	}
	
	private void LoadMp3(Stream stream)
	{
		using var mp3Reader = new Mp3FileReader(stream);
		using var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader);
		var buffer = new byte[waveStream.Length];
		waveStream.ReadExactly(buffer);
		
		var numChannels = waveStream.WaveFormat.Channels;
		var bitsPerSample = waveStream.WaveFormat.BitsPerSample;
		
		var format = numChannels switch
		{
			1 => bitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16,
			2 => bitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16,
			_ => throw new FormatException($"Unsupported channel count: {numChannels}")
		};
		
		duration = buffer.Length / (waveStream.WaveFormat.SampleRate * numChannels * (bitsPerSample / 8));
		this.buffer = CreateBuffer(buffer, format, waveStream.WaveFormat.SampleRate);
		source = CreateSource(this.buffer);
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
		
		duration = (int)vorbis.TotalTime.TotalSeconds;
		this.buffer = CreateBuffer(pcmStream.ToArray(),
								   vorbis.Channels == 1 ? BufferFormat.Mono16 : BufferFormat.Stereo16,
								   vorbis.SampleRate);
		source = CreateSource(this.buffer);
	}
	
	public void Play() => al.SourcePlay(source);
	
	public void Pause() => al.SourcePause(source);
	
	public void Stop() => al.SourceStop(source);
	
	void IResource.Load(Resource ressource)
	{
		var stream = ressource.stream;
		switch (ressource.extension)
		{
			case ".wav":
				LoadWav(stream);
				break;
			case ".mp3":
				LoadMp3(stream);
				break;
			case ".ogg":
				LoadOgg(stream);
				break;
			default:
				throw new NotSupportedException(
					$"Unsupported audio format: {ressource.extension}. Expected '.ogg' or '.wav'");
		}
	}
	
	void IAsset.Destroy()
	{
		al.DeleteBuffer(buffer);
		al.DeleteSource(source);
	}
	
	public void OnDebugPropertyRender()
	{
		al.GetSourceProperty(source, GetSourceInteger.SourceState, out var state);
		var isPlaying = state == (int)SourceState.Playing;
		var position = currentTime;
		
		ImGui.PushItemWidth(-1f);
		if (ImGui.SliderFloat("##timeline", ref position, 0, duration,
							  $@"{TimeSpan.FromSeconds(position):mm\:ss} / {TimeSpan.FromSeconds(duration):mm\:ss}"))
			currentTime = position;
		ImGui.PopItemWidth();
		
		if (ImGui.Button(isPlaying ? "Pause" : "Play"))
		{
			if (isPlaying)
				Pause();
			else
				Play();
		}
		ImGui.SameLine();
		
		if (ImGui.Button("Stop"))
		{
			Stop();
			currentTime = 0;
		}
	}
}