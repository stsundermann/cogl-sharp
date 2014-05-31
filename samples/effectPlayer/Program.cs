using System;
using System.IO;
using System.Timers;
using Cogl;
using Cogl.Gstreamer;
using GLib;
using Gst;

namespace CoglGst
{
	class MainClass
	{
		static Context context;
		static Framebuffer fb;
		static VideoSink sink;
		static Element playbin;
		static int onscreenWidth;
		static int onscreenHeight;
		static Rectangle videoOutput;
		static bool drawReady;
		static bool frameReady;
		static MainLoop mainLoop;
		static IEffect currentEffect;
		static VideoType optVideoType = VideoType.None;
		static String optVideoFile;
		static Timer timer;
		static Type[] effects = new Type[] { typeof(NoEffect), typeof(WaveyEffect), typeof (EdgeEffect), typeof (SquareEffect) };
		static Random random = new Random ();

		public enum VideoType
		{
			None,
			File
		}

		public static bool SetVideoType (VideoType type) {
			if (optVideoType != VideoType.None) {
				throw new Exception ("Only one video source can be specified");
				return false;
			}

			optVideoType = type;
			return true;
		}

		public static bool optFilenameCallback (string optionName, string value)
		{
			if (!SetVideoType (VideoType.File))
				return false;

			optVideoFile = value;
			return true;
		}

		public static bool busWatch (Gst.Bus bus, Gst.Message msg)
		{
			switch (msg.Type) {
			case Gst.MessageType.Eos:
				playbin.SeekSimple (Gst.Format.Time, Gst.SeekFlags.Flush, 0);
				break;
			case Gst.MessageType.Error:
				mainLoop.Quit ();
				break;
			}
			return true;
		}

		public static void Paint ()
		{
			currentEffect.Paint (fb, videoOutput);
			((Cogl.Onscreen)fb).SwapBuffers ();
		}

		public static void CheckDraw ()
		{
			if (drawReady && frameReady) {
				Paint ();
				drawReady = false;
				frameReady = false;
			}
		}

		public static void FrameCallback (Cogl.Onscreen onscreen, Cogl.FrameEvent evnt, Cogl.FrameInfo info) {
			if (evnt == Cogl.FrameEvent.Sync) {
				drawReady = true;
				CheckDraw ();
			}
		}

		public static void NewFrameCb (object obj, EventArgs args)
		{
			frameReady = true;
			CheckDraw ();
		}

		public static void ResizeCallback (Cogl.Onscreen onscreen, int width, int height)
		{
			onscreenWidth = width;
			onscreenHeight = height;

			fb.Orthographic (0, 0, width, height, -1, 100);

			if (sink.IsReady != 0)
				UpdateVideoOutput ();
		}

		public static void UpdateVideoOutput ()
		{
			Rectangle available;

			available.X = 0;
			available.Y = 0;
			available.Width = onscreenWidth;
			available.Height = onscreenHeight;

			sink.FitSize (available, ref videoOutput);
		}

		public static void SetUpPipeline (object obj, EventArgs args)
		{
			UpdateVideoOutput ();
			currentEffect.SetUpPipeline (sink);
		}

		public static void ClearEffect ()
		{
			currentEffect = null;
		}

		public static void SetEffect (IEffect effect)
		{
			ClearEffect ();

			sink.DefaultSample = 1;
			sink.FirstLayer = 0;

			effect.Init (context, sink);
			currentEffect = effect;

			if (sink.IsReady != 0)
				effect.SetUpPipeline (sink);
		}

		public static Context CreateContext ()
		{
			Renderer renderer;
			Display display;

			renderer = new Renderer ();

			display = new Display (renderer, null);

			return new Context (display);
		}

		public static Cogl.Onscreen CreateOnscreen ()
		{
			Onscreen onscreen = new Onscreen (context, 800, 600);

			onscreen.Resizable = true;
			onscreen.AddResizeCallback (ResizeCallback);
			onscreen.Allocate ();

			return onscreen;
		}

		public static void Main (string[] args)
		{
			Element pipeline;
			Gst.Bus bus;
			Onscreen onscreen;
			Source coglSource;

			Gst.Application.Init ();

			context = CreateContext ();
			onscreen = CreateOnscreen ();

			fb = onscreen;

			sink = new VideoSink (context);
			pipeline = new Gst.Pipeline ("gst-player");
			playbin = Gst.ElementFactory.Make ("playbin", "bin");

			if (optVideoType == VideoType.None) {
				optVideoFile = "http://docs.gstreamer.com/media/sintel_trailer-480p.webm";
				optVideoType = VideoType.None;
			}

			playbin ["video-sink"] = sink;
			((Gst.Bin)pipeline).Add (playbin);

			if (File.Exists (optVideoFile)) {
				playbin ["uri"] = new Uri (optVideoFile).AbsolutePath;
			} else
				playbin ["uri"] = optVideoFile;

			timer = new Timer (3000);
			timer.Elapsed += NextEffect;
			timer.Enabled = true;

			SetEffect ((IEffect) effects[random.Next (effects.Length)].GetConstructor (new Type[] {}).Invoke (new object[] {}));

			pipeline.SetState (Gst.State.Playing);
			bus = pipeline.Bus;
			bus.AddWatch (busWatch);

			mainLoop = new GLib.MainLoop ();

			coglSource = Cogl.Global.GlibSourceNew (context, (int)GLib.Priority.Default);
			coglSource.Attach (null);

			sink.PipelineReady += SetUpPipeline;
			sink.NewFrame += NewFrameCb;

			onscreen.AddFrameCallback (FrameCallback);

			drawReady = true;
			frameReady = false;

			onscreen.Show ();

			mainLoop.Run ();
		}

		public static void NextEffect (object source, ElapsedEventArgs e)
		{
			SetEffect ((IEffect) effects[random.Next (effects.Length)].GetConstructor (new Type[] {}).Invoke (new object[] {}));
		}
	}
}
