using System;
using Cogl;
using Cogl.Gstreamer;
using GLib;
using Gst;

namespace CoglBasicVideoPlayer
{
	class MainClass
	{
		static Onscreen fb;
		static Cogl.Pipeline borderPipeline;
		static Cogl.Pipeline videoPipeline;
		static VideoSink sink;
		static int onscreenWidth;
		static int onscreenHeight;
		static Rectangle videoOutput;
		static bool drawReady;
		static bool frameReady;
		static MainLoop mainLoop;

		static bool BusWatch (Gst.Bus bus, Message msg)
		{
			switch (msg.Type) {
			case MessageType.Eos:
				mainLoop.Quit ();
				break;
			case MessageType.Error:
				GException exception;
				string message;
				msg.ParseError (out exception, out message);
				Console.WriteLine ("[ERROR] " + exception.Message);
				mainLoop.Quit ();
				break;
			}
			return true;
		}

		static void Draw ()
		{
			Cogl.Pipeline current = sink.Pipeline;

			videoPipeline = current;

			if (videoOutput.X != 0) {
				float x = videoOutput.X;

				fb.DrawRectangle (borderPipeline, 0f, 0f, x, onscreenHeight);
				fb.DrawRectangle (borderPipeline, onscreenWidth - x, 0f, onscreenWidth, onscreenHeight);
				fb.DrawRectangle (videoPipeline, x, 0f, x + videoOutput.Width, onscreenHeight);
			} else if (videoOutput.Y != 0) {
				float y = videoOutput.Y;

				fb.DrawRectangle (borderPipeline, 0f, 0f, onscreenWidth, y);
				fb.DrawRectangle (borderPipeline, 0f, onscreenHeight - y, onscreenWidth, onscreenHeight);
				fb.DrawRectangle (videoPipeline, 0f, y, onscreenWidth, y + videoOutput.Height);
			} else {
				fb.DrawRectangle (videoPipeline, 0f, 0f, onscreenWidth, onscreenHeight);
			}
			fb.SwapBuffers ();
		}

		static void CheckDraw ()
		{
			if (drawReady && frameReady) {
				Draw ();
				drawReady = frameReady = false;
			}
		}

		static void FrameCallback (Onscreen onscreen, FrameEvent evnt, FrameInfo info)
		{
			if (evnt == FrameEvent.Sync) {
				drawReady = true;
				CheckDraw ();
			}
		}

		static void NewFrameCb (object obj, EventArgs args) {
			frameReady = true;
			CheckDraw ();
		}

		static void SetUpPipeline (object obj, EventArgs args)
		{
			videoPipeline = sink.Pipeline;

			videoPipeline.SetBlend ("RGBA = ADD (SRC_COLOR, 0)");

			ResizeCallback ((Onscreen)fb, onscreenWidth, onscreenHeight);

			fb.AddFrameCallback (FrameCallback);
			sink.NewFrame += NewFrameCb;
		}

		static bool MakePipelineForUri (Context ctx, Uri uri, out Element pipeline, out VideoSink sink)
		{
			Element bin;

			pipeline = new Gst.Pipeline ("gst-player");
			bin = ElementFactory.Make ("playbin", "bin");
			sink = new VideoSink (ctx);

			bin ["video-sink"] = sink;
			bin ["uri"] = uri.AbsoluteUri;

			((Bin)pipeline).Add (bin);

			return true;
		}

		static void ResizeCallback (Onscreen onscreen, int width, int height)
		{
			Rectangle available;

			onscreenWidth = width;
			onscreenHeight = height;

			fb.Orthographic (0, 0, width, height, -1, 100);

			if (videoPipeline == null)
				return;

			available.X = 0;
			available.Y = 0;
			available.Height = height;
			available.Width = width;

			sink.FitSize (available, ref videoOutput);
		}

		public static void Main (string[] args)
		{
			Context ctx;
			Onscreen onscreen;
			Element pipeline;
			Source coglSource;
			Gst.Bus bus;
			Uri uri;

			ctx = new Context (null);

			onscreen = new Onscreen (ctx, 800, 600);
			onscreen.Resizable = true;
			onscreen.AddResizeCallback (ResizeCallback);
			onscreen.Show ();

			fb = onscreen;
			fb.Orthographic (0, 0, 640, 480, -1, 100);

			borderPipeline = new Cogl.Pipeline (ctx);
			borderPipeline.SetColor4f (0.0f, 0.0f, 0.0f, 1.0f);
			borderPipeline.SetBlend ("RGBA = ADD (SRC_COLOR, 0)");
			Gst.Application.Init ();

			if (args.Length == 0)
				uri = new Uri("http://docs.gstreamer.com/media/sintel_trailer-480p.webm");
			else
				uri = new Uri (args [1]);

			if (!MakePipelineForUri (ctx, uri, out pipeline, out sink)) {
				Console.WriteLine ("Error creating pipeline");
				return;
			}

			pipeline.SetState (State.Playing);
			bus = pipeline.Bus;
			bus.AddWatch (BusWatch);

			mainLoop = new MainLoop ();
			coglSource = Cogl.Global.GlibSourceNew (ctx, (int)Priority.Default);
			coglSource.Attach (null);

			sink.PipelineReady += SetUpPipeline;

			drawReady = true;
			frameReady = false;

			mainLoop.Run ();
		}
	}
}
