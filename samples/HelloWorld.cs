using System;
using System.Collections.Generic;
using Cogl;
using GLib;

namespace CoglTest
{
	class MainClass
	{
		static Context ctx;
		static Primitive triangle;
		static Pipeline pipeline;
		static Onscreen onscreen;

		static uint redraw_idle;
		static bool is_dirty;
		static bool draw_ready;

		static void Main (string[] args)
		{
			VertexP2C4[] triangle_vertices = new VertexP2C4[] {
				new VertexP2C4 () { X =  0.0f, Y =  0.7f, R = 0xff, G = 0x00, B = 0x00, A = 0xff },
				new VertexP2C4 () { X = -0.7f, Y = -0.7f, R = 0x00, G = 0xff, B = 0x00, A = 0xff },
				new VertexP2C4 () { X =  0.7f, Y = -0.7f, R = 0x00, G = 0x00, B = 0xff, A = 0xff }
			};
					
			Source cogl_source;
			MainLoop loop;

			redraw_idle = 0;
			is_dirty = false;
			draw_ready = true;

			try {
				ctx = new Context (null);
			} catch (GException) {
				Console.WriteLine ("Failed to create context");
				return;
			}

			onscreen = new Onscreen (ctx, 640, 480);
			onscreen.Show ();
			onscreen.Resizable = true;

			triangle = new Primitive (ctx,
				VerticesMode.Triangles,
				triangle_vertices);
			pipeline = new Pipeline (ctx);

			cogl_source = Cogl.Global.GlibSourceNew (ctx, (int)Priority.Default);
			cogl_source.Attach (null);

			onscreen.AddFrameCallback (frame_event_cb);
			onscreen.AddDirtyCallback (dirty_cb);

			paint_cv ();

			loop = new MainLoop ();
			loop.Run ();
		}

		static bool paint_cv () {

			redraw_idle = 0;
			is_dirty = false;
			draw_ready = false;

			onscreen.Clear4f ((ulong)BufferBit.Color, 0, 0, 0, 1);
			triangle.Draw (onscreen, pipeline);

			onscreen.SwapBuffers ();

			return false;
		}

		static void maybe_redraw() {
			if (is_dirty && draw_ready && redraw_idle == 0)
				redraw_idle = Idle.Add (paint_cv);
		}

		static void frame_event_cb (Onscreen onscreen, FrameEvent evnt, FrameInfo info) {
			if (evnt == FrameEvent.Sync) {
				draw_ready = true;
				maybe_redraw ();
			}
		}

		static void dirty_cb (Onscreen onscreen, OnscreenDirtyInfo info) {
			is_dirty = true;
			maybe_redraw ();
		}
	}

}