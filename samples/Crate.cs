using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Cogl;
using GLib;

namespace CoglCrate
{
	class MainClass
	{
		static Onscreen fb;
		static int framebuffer_width;
		static int framebuffer_height;

		static Matrix view;
		
		static Indices indices;
		static Primitive prim;
		static Texture texture;
		static Pipeline crate_pipeline;

		static Stopwatch timer;

		static uint redraw_idle;
		static bool is_dirty;
		static bool draw_ready;

		static Matrix identity;
		static Color white;

		static VertexP3T2[] vertices = new VertexP3T2[] {
			new VertexP3T2() { X = -1.0f, Y = -1.0f, Z = 1.0f, S = 0.0f, T = 1.0f},
			new VertexP3T2() { X =  1.0f, Y = -1.0f, Z = 1.0f, S = 1.0f, T = 1.0f},
			new VertexP3T2() { X =  1.0f, Y =  1.0f, Z = 1.0f, S = 1.0f, T = 0.0f},
			new VertexP3T2() { X = -1.0f, Y =  1.0f, Z = 1.0f, S = 0.0f, T = 0.0f},

			new VertexP3T2() { X = -1.0f, Y = -1.0f, Z = -1.0f, S = 1.0f, T = 0.0f},
			new VertexP3T2() { X = -1.0f, Y =  1.0f, Z = -1.0f, S = 1.0f, T = 1.0f},
			new VertexP3T2() { X =  1.0f, Y =  1.0f, Z = -1.0f, S = 0.0f, T = 1.0f},
			new VertexP3T2() { X =  1.0f, Y = -1.0f, Z = -1.0f, S = 0.0f, T = 0.0f},

			new VertexP3T2() { X = -1.0f, Y =  1.0f, Z = -1.0f, S = 0.0f, T = 1.0f},
			new VertexP3T2() { X = -1.0f, Y =  1.0f, Z =  1.0f, S = 0.0f, T = 0.0f},
			new VertexP3T2() { X =  1.0f, Y =  1.0f, Z =  1.0f, S = 1.0f, T = 0.0f},
			new VertexP3T2() { X =  1.0f, Y =  1.0f, Z = -1.0f, S = 1.0f, T = 1.0f},

			new VertexP3T2() { X = -1.0f, Y = -1.0f, Z = -1.0f, S = 1.0f, T = 1.0f},
			new VertexP3T2() { X =  1.0f, Y = -1.0f, Z = -1.0f, S = 0.0f, T = 1.0f},
			new VertexP3T2() { X =  1.0f, Y = -1.0f, Z =  1.0f, S = 0.0f, T = 0.0f},
			new VertexP3T2() { X = -1.0f, Y = -1.0f, Z =  1.0f, S = 1.0f, T = 0.0f},

			new VertexP3T2() { X =  1.0f, Y = -1.0f, Z = -1.0f, S = 1.0f, T = 0.0f},
			new VertexP3T2() { X =  1.0f, Y =  1.0f, Z = -1.0f, S = 1.0f, T = 1.0f},
			new VertexP3T2() { X =  1.0f, Y =  1.0f, Z =  1.0f, S = 0.0f, T = 1.0f},
			new VertexP3T2() { X =  1.0f, Y = -1.0f, Z =  1.0f, S = 0.0f, T = 0.0f},

			new VertexP3T2() { X = -1.0f, Y = -1.0f, Z = -1.0f, S = 0.0f, T = 0.0f},
			new VertexP3T2() { X = -1.0f, Y = -1.0f, Z =  1.0f, S = 1.0f, T = 0.0f},
			new VertexP3T2() { X = -1.0f, Y =  1.0f, Z =  1.0f, S = 1.0f, T = 1.0f},
			new VertexP3T2() { X = -1.0f, Y =  1.0f, Z = -1.0f, S = 0.0f, T = 1.0f},
		};

		static bool paint () {
			float rotation;

			redraw_idle = 0;
			is_dirty = false;
			draw_ready = false;

			fb.Clear4f ((ulong)(BufferBit.Color | BufferBit.Depth), 0, 0, 0, 1);
			fb.PushMatrix ();
			fb.Translate (framebuffer_width / 2f, framebuffer_height / 2f, 0);
			fb.Scale (75, 75, 75);

			rotation = (float)timer.Elapsed.TotalSeconds * 60.0f;

			fb.Rotate (rotation, 0, 0, 1);
			fb.Rotate (rotation, 0, 1, 0);
			fb.Rotate (rotation, 1, 0, 0);

			prim.Draw (fb, crate_pipeline);

			fb.PopMatrix ();

			Onscreen onscrn = new Onscreen (fb.Handle);
			onscrn.SwapBuffers ();

			return true;
		}

		static void frame_event_cb (Onscreen screen, FrameEvent evnt, FrameInfo info) {
			if (evnt == FrameEvent.Sync) {
				draw_ready = true;
				maybe_redraw ();
			}
		}

		public static void Main (string[] args)
		{
			Context ctx;
			float fovy, aspect, z_near, z_2d, z_far;
			DepthState depth_state = DepthState.Zero;

			redraw_idle = 0;
			is_dirty = false;
			draw_ready = true;

			try {
				ctx = new Context (null);
			} catch (GException) {
				Console.WriteLine ("Failed to create context");
				return;
			}

			fb = new Onscreen (ctx, 640, 480);
			framebuffer_width = fb.Width;
			framebuffer_height = fb.Height;

			timer = new Stopwatch ();
			timer.Start ();
			
			fb.Show ();

			fb.SetViewport (0, 0, framebuffer_width, framebuffer_height);

			fovy = 60;
			aspect = (float)framebuffer_width / (float)framebuffer_height;
			z_near = 0.1f;
			z_2d = 1000;
			z_far = 2000;

			fb.Perspective (fovy, aspect, z_near, z_far);

			view.InitIdentity ();
			view.View2dInPerspective (fovy, aspect, z_near, z_2d, framebuffer_width, framebuffer_height);
			fb.SetModelviewMatrix (view);

			identity.InitIdentity ();
			white.InitFrom4ub (0xff, 0xff, 0xff, 0xff);

			indices = Cogl.Global.GetRectangleIndices (ctx, 6);

			prim = new Primitive (ctx, VerticesMode.Triangles, vertices);
			prim.SetIndices (indices, 6 * 6);

			texture = new Texture2D (ctx, "crate.jpg");

			crate_pipeline = new Pipeline (ctx);
			crate_pipeline.SetLayerTexture (0, texture);

			depth_state.Init ();
			General.DepthTestEnabled = true;

			crate_pipeline.SetDepthState (depth_state);

			General.PushFramebuffer (fb);

			Onscreen onscrn = new Onscreen (fb.Handle);

			onscrn.AddFrameCallback (frame_event_cb);
			onscrn.AddDirtyCallback (dirty_cb);

			onscrn.SwapBuffers ();
				
			MainLoop loop = new MainLoop ();

			Source source = Cogl.Global.GlibSourceNew (ctx, (int)Priority.High);
			source.Attach (loop.Context);

			loop.Run ();
		}

		static void maybe_redraw() {
			if (is_dirty && draw_ready && redraw_idle == 0)
				redraw_idle = Idle.Add (paint);
		}

		static void dirty_cb (Onscreen onscreen, OnscreenDirtyInfo info) {
			is_dirty = true;
			maybe_redraw ();
		}
	}
}
