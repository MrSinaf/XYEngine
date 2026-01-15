using System.Diagnostics;

namespace XYEngine.Debugs.Windows;

internal class DebugPerformance : IDebugWindow, IDebugMainBar
{
	public bool visible { get; set; }
	public int priority => 0;
	
	private static readonly float[] fpsHistory = new float[100];
	private static readonly float[] memoryHistory = new float[100];
	private static readonly float[] frameTimeHistory = new float[100];
	private static int historyIndex;
	
	private static float time;
	
	public void OnDebugWindowRender(string name)
	{
		var isOpen = visible;
		if (ImGui.Begin(name, ref isOpen, ImGuiWindowFlags.AlwaysAutoResize))
		{
			// ImGui.SetWindowSize(new Vector2(300, 200));
			time += Time.delta;
			if (time > 0.05F)
			{
				time = 0;
				var currentFps = (int)(1 / Time.delta);
				var currentProcess = Process.GetCurrentProcess();
				var totalMemoryUsed = currentProcess.WorkingSet64 / 1048576F;
				
				var frameTimeMs = Time.delta * 1000;
				
				fpsHistory[historyIndex] = currentFps;
				memoryHistory[historyIndex] = totalMemoryUsed;
				frameTimeHistory[historyIndex] = frameTimeMs;
				historyIndex = (historyIndex + 1) % fpsHistory.Length;
			}
			
			float minFps = float.MaxValue, maxFps = 0, avgFps = 0;
			float minMem = float.MaxValue, maxMem = 0, avgMem = 0;
			float minFrameTime = float.MaxValue, maxFrameTime = 0, avgFrameTime = 0;
			var count = 0;
			
			for (var i = 0; i < fpsHistory.Length; i++)
			{
				if (fpsHistory[i] <= 0) continue;
				
				count++;
				minFps = Math.Min(minFps, fpsHistory[i]);
				maxFps = Math.Max(maxFps, fpsHistory[i]);
				avgFps += fpsHistory[i];
				
				minMem = Math.Min(minMem, memoryHistory[i]);
				maxMem = Math.Max(maxMem, memoryHistory[i]);
				avgMem += memoryHistory[i];
				
				minFrameTime = Math.Min(minFrameTime, frameTimeHistory[i]);
				maxFrameTime = Math.Max(maxFrameTime, frameTimeHistory[i]);
				avgFrameTime += frameTimeHistory[i];
			}
			
			if (count > 0)
			{
				avgFps /= count;
				avgMem /= count;
				avgFrameTime /= count;
			}
			
			ImGui.PlotLines("##fps", ref fpsHistory[0], fpsHistory.Length, historyIndex,
							$"{(count > 0 ?
								fpsHistory[(historyIndex - 1 + fpsHistory.Length) % fpsHistory.Length] : 0):F0} FPS", 0,
							maxFps * 1.2f, new Vector2(ImGui.GetContentRegionAvail().x, 60));
			ImGui.PlotLines("##frameTime", ref frameTimeHistory[0], frameTimeHistory.Length, historyIndex,
							$"{(count > 0 ?
								frameTimeHistory[(historyIndex - 1 + frameTimeHistory.Length) %
												 frameTimeHistory.Length] : 0):F2} ms",
							0, maxFrameTime * 1.2f, new Vector2(ImGui.GetContentRegionAvail().x / 2, 60));
			ImGui.SameLine();
			ImGui.PlotLines("##memory", ref memoryHistory[0], memoryHistory.Length, historyIndex,
							$"{(count > 0 ? 
								memoryHistory[(historyIndex - 1 + memoryHistory.Length) % 
											  memoryHistory.Length] : 0):F2} MB",
							0, maxMem * 1.2f, new Vector2(ImGui.GetContentRegionAvail().x, 60));
			
			if (ImGui.BeginTable("stats_table", 4,
								 ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedSame))
			{
				ImGui.TableSetupColumn("Metric");
				ImGui.TableSetupColumn("Min");
				ImGui.TableSetupColumn("Max");
				ImGui.TableSetupColumn("Avg");
				ImGui.TableHeadersRow();
				
				ImGui.TableNextRow();
				ImGui.TableSetColumnIndex(0);
				ImGui.Text("FPS");
				ImGui.TableSetColumnIndex(1);
				ImGui.Text(count > 0 ? $"{minFps:F0}" : "-");
				ImGui.TableSetColumnIndex(2);
				ImGui.Text(count > 0 ? $"{maxFps:F0}" : "-");
				ImGui.TableSetColumnIndex(3);
				ImGui.Text(count > 0 ? $"{avgFps:F0}" : "-");
				
				ImGui.TableNextRow();
				ImGui.TableSetColumnIndex(0);
				ImGui.Text("Memory (Mo)");
				ImGui.TableSetColumnIndex(1);
				ImGui.Text(count > 0 ? $"{minMem:F2}" : "-");
				ImGui.TableSetColumnIndex(2);
				ImGui.Text(count > 0 ? $"{maxMem:F2}" : "-");
				ImGui.TableSetColumnIndex(3);
				ImGui.Text(count > 0 ? $"{avgMem:F2}" : "-");
				
				ImGui.TableNextRow();
				ImGui.TableSetColumnIndex(0);
				ImGui.Text("Frame (ms)");
				ImGui.TableSetColumnIndex(1);
				ImGui.Text(count > 0 ? $"{minFrameTime:F2}" : "-");
				ImGui.TableSetColumnIndex(2);
				ImGui.Text(count > 0 ? $"{maxFrameTime:F2}" : "-");
				ImGui.TableSetColumnIndex(3);
				ImGui.Text(count > 0 ? $"{avgFrameTime:F2}" : "-");
				
				ImGui.EndTable();
				
				ImGui.Separator();
				ImGui.Spacing();
				XYDebug.ShowValue("Runtime", TimeSpan.FromSeconds(Time.total).ToString(@"hh\:mm\:ss"));
			}
		}
		ImGui.End();
		visible = isOpen;
	}
	
	public void OnDebugMainBarRender()
	{
		if (ImGui.BeginMenu("Tools"))
		{
			if (ImGui.MenuItem("Performance"))
				visible = !visible;
			
			ImGui.EndMenu();
		}
	}
}