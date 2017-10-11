using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APNG : MonoBehaviour
{
	public enum CompressionLevel
	{
		Min, Default, Max
	}

	public Camera capture;

	[Range(1, 30)]
	public int fps = 10;
	public int totalFrames = 30;

	public Color backgroundColor = Color.magenta;
	public bool isTransparent = true;
	public Vector2 resolution = new Vector2(320, 240);

	public CompressionLevel compressionLevel = CompressionLevel.Default;

	[HideInInspector]
	public bool isRecording = false;

	public void BeginRecord()
	{
		if (isRecording)
		{
			UnityEngine.Debug.LogError("[APNG] Already recording");
			return;
		}
		if (Application.isEditor == false)
			UnityEngine.Debug.LogWarning("[APNG] APNG is not tested under standalone player mode.");
		if (compressionLevel == CompressionLevel.Max)
			UnityEngine.Debug.LogWarning("[APNG] Max compressionLevel may not be effective option.");

		StartCoroutine(RecordFunc());
	}
	IEnumerator RecordFunc()
	{
		isRecording = true;

		var rt = new RenderTexture(
			(int)resolution.x, (int)resolution.y, 24, RenderTextureFormat.ARGB32);
		var tex2d = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
		var idx = 0;

		EnsureTempDir();
		SetupCamera(rt);

		UnityEngine.Debug.Log("[APNG] BeginCapture");
		for (int i = 0; i < totalFrames; i++)
		{
			capture.Render();

			RenderTexture.active = rt;
			tex2d.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			tex2d.Apply();
			RenderTexture.active = null;

			File.WriteAllBytes(GetSavePath(i), tex2d.EncodeToPNG());

			yield return new WaitForSecondsRealtime(1.0f / fps);

			idx++;
		}
		UnityEngine.Debug.Log("[APNG] EndCapture");

		yield return null;

		GenerateAPNG();

		Directory.Delete("APNG", true);

		isRecording = false;
	}

	private void SetupCamera(RenderTexture rt)
	{
		capture.backgroundColor = backgroundColor;
		if (isTransparent)
			capture.backgroundColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0);
		else
			capture.backgroundColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 1);

		capture.targetTexture = rt;
	}

	private void EnsureTempDir()
	{
		if (Directory.Exists("APNG") == false)
			Directory.CreateDirectory("APNG");
	}
	private string GetSavePath(int idx)
	{
		return "APNG/" + idx.ToString() + ".png";
	}
		
	private void GenerateAPNG()
	{
		UnityEngine.Debug.Log("[APNG] BeginGenerateAPNG");

		var ps = new ProcessStartInfo();
		ps.FileName = Application.dataPath + "/APNG/apngasm.exe";
		ps.Arguments = "output.png APNG/*.png 1 " + fps.ToString();
		ps.RedirectStandardOutput = true;
		ps.RedirectStandardError = true;
		ps.UseShellExecute = false;
		ps.WindowStyle = ProcessWindowStyle.Hidden;

		if (compressionLevel == CompressionLevel.Max)
			ps.Arguments += " -z2";
		if (compressionLevel == CompressionLevel.Min)
			ps.Arguments += " -z0";

		var p = Process.Start(ps);
		p.WaitForExit(5000);

		UnityEngine.Debug.Log(p.StandardOutput.ReadToEnd());
		UnityEngine.Debug.Log(p.StandardError.ReadToEnd());

		UnityEngine.Debug.Log("[APNG] EndGenerateAPNG");
	}
}
