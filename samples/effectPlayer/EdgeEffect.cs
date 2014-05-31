using System;
using Cogl;
using Cogl.Gstreamer;
using GLib;
using Gst;

namespace CoglGst
{
	public class EdgeEffect : IEffect
	{
		Context context;
		VideoSink sink;
		Borders borders;

		float lastOutputWidth;
		float lastOutputHeight;

		Cogl.Pipeline basePipeline;
		Cogl.Pipeline pipeline;

		String shader_declarations =
			"uniform vec2 pixel_step;\n" +
			"\n" +
			"float\n" +
			"get_grey (vec2 coords)\n" +
			"{\n" +
			"  vec4 color = cogl_gst_sample_video0 (coords);\n" +
			"  return dot (color.rgb, vec3 (0.299, 0.587, 0.114));\n" +
			"}\n";

		string shader_source =
			"float top_left = get_grey (cogl_tex_coord0_in.st - pixel_step);\n" +
			"float top = get_grey (cogl_tex_coord0_in.st - vec2 (0.0, pixel_step.y));\n" +
			"float top_right =\n" +
			"  get_grey (cogl_tex_coord0_in.st + pixel_step * vec2 (1.0, -1.0));\n" +
			"float bottom_left =\n" +
			"  get_grey (cogl_tex_coord0_in.st + pixel_step * vec2 (-1.0, 1.0));\n" +
			"float bottom =\n" +
			"  get_grey (cogl_tex_coord0_in.st + vec2 (0.0, pixel_step.y));\n" +
			"float bottom_right = get_grey (cogl_tex_coord0_in.st + pixel_step);\n" +
			"float left = get_grey (cogl_tex_coord0_in.st - vec2 (pixel_step.x, 0.0));\n" +
			"float right = get_grey (cogl_tex_coord0_in.st + vec2 (pixel_step.x, 0.0));\n" +
			"float h = (top_left - top_right + bottom_left - bottom_right +\n" +
			"           (left - right) * 2.0);\n" +
			"float v = (top_left - bottom_left + top_right - bottom_right +\n" +
			"           (top - bottom) * 2.0);\n" +
			"cogl_color_out *= abs (h) + abs (v);\n";

		public void Init (Cogl.Context context, VideoSink sink)
		{
			this.context = context;
			this.sink = sink;
			this.borders = new Borders (context);

			CreatePipeline ();
			sink.DefaultSample = 1;
		}

		public void SetUpPipeline (VideoSink sink)
		{
			sink.SetupPipeline (pipeline);
			lastOutputWidth = lastOutputHeight = 0;
		}

		public void Paint (Framebuffer fb, Rectangle videoOutput)
		{
			sink.AttachFrame (pipeline);

			if (lastOutputWidth != videoOutput.Width || lastOutputHeight != videoOutput.Height) {
				int location = pipeline.GetUniformLocation ("pixel_step");

				if (location != -1) {
					float[] value = { 1f / videoOutput.Width, 1f / videoOutput.Height };

					pipeline.SetUniformFloat (location, 2, 1, value);
				}
				lastOutputWidth = videoOutput.Width;
				lastOutputHeight = videoOutput.Height;
			}

			borders.Draw (fb, videoOutput);
			fb.DrawRectangle (pipeline, videoOutput.X, videoOutput.Y, videoOutput.X + videoOutput.Width, videoOutput.Y + videoOutput.Height);
		}

		public void CreatePipeline ()
		{
			pipeline = new Cogl.Pipeline (context);
			Snippet snippet = new Cogl.Snippet (Cogl.SnippetHook.Fragment, shader_declarations, shader_source);
			pipeline.SetBlend ("RGBA = ADD (SRC_COLOR, 0)");
			pipeline.AddSnippet (snippet);
			snippet.Dispose ();
		}
	}
}

