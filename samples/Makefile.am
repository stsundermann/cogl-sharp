SUBDIRS = effectPlayer

TARGETS = cogl-hello-world.exe \
    crate.exe

if COGLGSTREAMER
   TARGETS += basic-video-player.exe
endif

DEBUGS = $(addsuffix .mdb, $(TARGETS))
assemblies = 					\
	$(top_builddir)/out/cogl-sharp.dll

references=$(addprefix -r:, $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS) $(DEBUGS)

cogl-hello-world.exe: $(srcdir)/HelloWorld.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:cogl-hello-world.exe $(references) $(GTK_SHARP_LIBS) $(srcdir)/HelloWorld.cs

basic-video-player.exe: $(srcdir)/HelloWorld.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:basic-video-player.exe $(references) $(GTK_SHARP_LIBS) $(GSTREAMERSHARP_LIBS) $(srcdir)/BasicVideoPlayer.cs

crate.exe: $(srcdir)/Crate.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:crate.exe $(references) $(GTK_SHARP_LIBS) $(srcdir)/Crate.cs

EXTRA_DIST = 				\
	HelloWorld.cs \
    BasicVideoPlayer.cs \
    Crate.cs \
    crate.jpg
