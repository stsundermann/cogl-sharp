using System;
using Cogl;
using Cogl.Gstreamer;
using GLib;
using Gst;

namespace CoglGst
{
	public class SquareEffect : IEffect
	{
		Cogl.Context context;
		VideoSink sink;
		Cogl.Pipeline pipeline;
		Borders borders;

		string shader_source =
			/* This splits the video into a grid of squares and then flips the
   * individual squares */
			"const float N_SQUARES = 8.0;\n" +
			"vec2 square_num = floor (cogl_tex_coord0_in.st * N_SQUARES);\n" +
			"vec2 in_square = fract ((1.0 - cogl_tex_coord0_in.st) * N_SQUARES);\n" +
			"vec2 coords = (square_num + in_square) / N_SQUARES;\n" +
			"cogl_color_out *= cogl_gst_sample_video0 (coords);\n";

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

		public void Paint (Framebuffer fb, Rectangle videoOutput)
		{
			sink.AttachFrame (pipeline);
			borders.Draw (fb, videoOutput);
			fb.DrawRectangle (pipeline, videoOutput.X, videoOutput.Y, videoOutput.X + videoOutput.Width, videoOutput.Y + videoOutput.Height);
		}

		public void CreatePipeline ()
		{
			pipeline = new Cogl.Pipeline (context);
			Cogl.Snippet snippet = new Snippet (SnippetHook.Fragment, null, shader_source);
			pipeline.SetBlend ("RGBA = ADD (SRC_COLOR, 0)");
			pipeline.AddSnippet (snippet);
			snippet.Dispose ();
		}
	}
}

