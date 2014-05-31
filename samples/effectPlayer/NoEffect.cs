using System;
using Cogl;
using Cogl.Gstreamer;
using GLib;
using Gst;

namespace CoglGst
{
	public class NoEffect : IEffect
	{
		Context context;
		VideoSink sink;
		Borders borders;

		public void Init (Context context, VideoSink sink)
		{
			this.context = context;
			this.sink = sink;
			this.borders = new Borders (context);
		}

		public void SetUpPipeline (VideoSink sink)
		{
			sink.Pipeline.SetBlend ("RGBA = ADD (SRC_COLOR, 0)");
		}

		public void Paint (Framebuffer fb, Rectangle videoOutput)
		{
			Cogl.Pipeline pipeline = sink.Pipeline;

			borders.Draw (fb, videoOutput);
			fb.DrawRectangle (pipeline, videoOutput.X, videoOutput.Y,
				videoOutput.X + videoOutput.Width, videoOutput.Y + videoOutput.Height);
		}
	}
}

