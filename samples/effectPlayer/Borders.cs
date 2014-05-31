using System;
using Cogl;
using Cogl.Gstreamer;
using GLib;
using Gst;

namespace CoglGst
{
	public class Borders : IDisposable
	{
		Cogl.Pipeline pipeline;

		public Borders (Cogl.Context context)
		{
			pipeline = new Cogl.Pipeline (context);
			pipeline.SetColor4f (0.0f, 0.0f, 0.0f, 1.0f);
			pipeline.SetBlend ("RGBA = ADD (SRC_COLOR, 0)");
		}

		public void Draw (Cogl.Framebuffer fb, Rectangle videoOutput) {
			int fbHeight = fb.Height;
			int fbWidth = fb.Width;

			if (videoOutput.X > 0) {
				float x = videoOutput.X;

				fb.DrawRectangle (pipeline, 0, 0, x, fbHeight);
				fb.DrawRectangle (pipeline, fbWidth - x, 0, fbWidth, fbHeight);
			} else if (videoOutput.Y > 0) {
				float y = videoOutput.Y;

				fb.DrawRectangle (pipeline, 0, 0, fbWidth, y);
				fb.DrawRectangle (pipeline, 0, fbHeight - y, fbWidth, fbHeight);
			}
		}

		public void Dispose ()
		{
			pipeline.Dispose ();
		}
	}
}

