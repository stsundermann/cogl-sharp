using System;
using Cogl;
using Cogl.Gstreamer;
using GLib;
using Gst;

namespace CoglGst
{
	public interface IEffect
	{
		void Init (Cogl.Context context, VideoSink sink);
		void SetUpPipeline (VideoSink sink);
		void Paint (Framebuffer fb, Rectangle videoOutput);
	}
}

