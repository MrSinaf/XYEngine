using Silk.NET.OpenAL;
using XYEngine.Debugs;
using XYEngine.Resources;
using XYEngine.Utils;
using static XYEngine.Audio;

namespace XYEngine;

public class AudioSource : Component, IDebugProperty, IDisposable
{
	private readonly uint source;
	public AudioClip clip { get; private set; }
	
	#region Properties
	
	public float volume
	{
		get
		{
			al.GetSourceProperty(source, SourceFloat.Gain, out var gain);
			return gain;
		}
		set
		{
			al.SetSourceProperty(source, SourceFloat.Gain, MathUtils.Range(value, 0, 1));
			CheckError();
		}
	}
	
	public float pitch
	{
		get
		{
			al.GetSourceProperty(source, SourceFloat.Pitch, out var pitch);
			return pitch;
		}
		set
		{
			al.SetSourceProperty(source, SourceFloat.Pitch, Math.Max(value, 0.01f));
			CheckError();
		}
	}
	
	public bool isLooping
	{
		get
		{
			al.GetSourceProperty(source, SourceBoolean.Looping, out var looping);
			return looping;
		}
		set
		{
			al.SetSourceProperty(source, SourceBoolean.Looping, value);
			CheckError();
		}
	}
	
	public bool isPlaying
	{
		get
		{
			al.GetSourceProperty(source, GetSourceInteger.SourceState, out var state);
			return state == (int)SourceState.Playing;
		}
	}
	
	public bool isPaused
	{
		get
		{
			al.GetSourceProperty(source, GetSourceInteger.SourceState, out var state);
			return state == (int)SourceState.Paused;
		}
	}
	
	public float currentTime
	{
		get
		{
			if (clip == null) return 0;
			
			al.GetSourceProperty(source, SourceFloat.SecOffset, out var time);
			return time;
		}
		set
		{
			if (clip != null)
				al.SetSourceProperty(source, SourceFloat.SecOffset, MathUtils.Range(value, 0, clip.duration));
		}
	}
	
	#endregion
	
	public AudioSource()
	{
		source = al.GenSource();
		CheckError();
		
		volume = 1.0f;
		pitch = 1.0f;
	}
	
	public void SetClip(AudioClip newClip)
	{
		if (newClip == null)
		{
			al.SetSourceProperty(source, SourceInteger.Buffer, 0);
			clip = null;
		}
		else
		{
			if (isPlaying)
				Stop();
			
			al.SetSourceProperty(source, SourceInteger.Buffer, newClip.buffer);
			clip = newClip;
			
			CheckError();
		}
	}
	
	public void Play()
	{
		if (clip == null)
		{
			XY.Log("Trying to play AudioSource without a clip", TypeLog.Warning);
			return;
		}
		
		al.SourcePlay(source);
		CheckError();
	}
	
	public void Pause()
	{
		al.SourcePause(source);
		CheckError();
	}
	
	public void Stop()
	{
		al.SourceStop(source);
		CheckError();
	}
	
	public override void Destroy() => Dispose();
	
	void IDisposable.Dispose()
	{
		al.DeleteSource(source);
		GC.SuppressFinalize(this);
	}
	
	void IDebugProperty.OnDebugPropertyRender()
	{
		if (clip == null)
		{
			ImGui.Text("No audio clip assigned");
			return;
		}
		
		var position = currentTime;
		XYDebug.ShowValue("Clip", clip.assetPath);
		
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
		ImGui.SameLine();
		var loop = isLooping;
		if (ImGui.Checkbox("Loop", ref loop))
			isLooping = loop;
		
		ImGui.PushItemWidth(-1f);
		var formatTime = $"{ TimeSpan.FromSeconds(position):mm:ss} / {TimeSpan.FromSeconds(clip.duration):mm:ss}";
		if (ImGui.SliderFloat("##timeline", ref position, 0, clip.duration, formatTime))
			currentTime = position;
		ImGui.PopItemWidth();
		
		ImGui.Columns(2, "AudioParams", false);
		ImGui.SetColumnWidth(0, 60);
		
		ImGui.AlignTextToFramePadding();
		ImGui.Text("Volume");
		ImGui.NextColumn();
		ImGui.PushItemWidth(-1f);
		var vol = volume;
		if (ImGui.SliderFloat("##volume", ref vol, 0, 1))
			volume = vol;
		ImGui.PopItemWidth();
		ImGui.NextColumn();
		
		ImGui.AlignTextToFramePadding();
		ImGui.Text("Pitch");
		ImGui.NextColumn();
		ImGui.PushItemWidth(-1f);
		var p = pitch;
		if (ImGui.SliderFloat("##pitch", ref p, 0.1f, 3.0f))
			pitch = p;
		ImGui.PopItemWidth();
		ImGui.NextColumn();
		ImGui.Columns();
	}
	
	private static void CheckError()
	{
		var error = al.GetError();
		if (error != AudioError.NoError)
			throw new Exception(error.ToString());
	}
	
	~AudioSource() => Dispose();
}