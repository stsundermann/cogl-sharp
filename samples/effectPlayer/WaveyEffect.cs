using System;
using Cogl;
using Cogl.Gstreamer;
using GLib;
using Gst;

namespace CoglGst
{
	public class WaveyEffect : IEffect
	{
		String shaderSource =   
			"const float PI = " + Math.PI + ";\n" +
			"vec2 coords = cogl_tex_coord0_in.st;\n" +
			"coords += sin (coords * PI * 2.0 * 8.0) / 30.0;\n" +
			"cogl_color_out *= cogl_gst_sample_video0 (coords);\n";

		Cogl.Context context;
		VideoSink sink;

		Cogl.Pipeline basePipeline;
		Cogl.Pipeline pipeline;

		Borders borders;

		public void Init (Cogl.Context context, VideoSink sink)
		{
			this.context = context;
			this.sink = sink;
			this.borders = new Borders (context);

			CreatePipeline ();
			sink.DefaultSample = 0;
		}

		public void SetUpPipeline (VideoSink sink)
		{
			sink.SetupPipeline (pipeline);
		}

		public void Paint (Cogl.Framebuffer fb, Rectangle videoOutput)
		{
			sink.AttachFrame (pipeline);
			borders.Draw (fb, videoOutput);

			fb.DrawRectangle (pipeline, videoOutput.X, videoOutput.Y, videoOutput.X + videoOutput.Width, videoOutput.Y + videoOutput.Height);
		}

		public void CreatePipeline ()
		{
			Snippet snippet;

			pipeline = new Cogl.Pipeline (context);
			pipeline.SetBlend ("RGBA = ADD (SRC_COLOR, 0)");

			snippet = new Cogl.Snippet (SnippetHook.Fragment, null, shaderSource);
			pipeline.AddSnippet (snippet);
			snippet.Dispose ();

			basePipeline = pipeline;
		}
	}
}

