SUBDIRS = 

TARGETS =

if COGLGSTREAMER
   TARGETS += effect-player.exe
endif

DEBUGS = $(addsuffix .mdb, $(TARGETS))
assemblies = 					\
	$(top_builddir)/out/cogl-sharp.dll

references=$(addprefix -r:, $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS) $(DEBUGS)

effect-player.exe: $(srcdir)/Program.cs $(srcdir)/Borders.cs $(srcdir)/EdgeEffect.cs $(srcdir)/IEffect.cs $(srcdir)/NoEffect.cs $(srcdir)/SquareEffect.cs $(srcdir)/WaveyEffect.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:effect-player.exe $(references) $(GTK_SHARP_LIBS) $(GSTREAMERSHARP_LIBS) $(srcdir)/Program.cs $(srcdir)/Borders.cs $(srcdir)/EdgeEffect.cs $(srcdir)/IEffect.cs $(srcdir)/NoEffect.cs $(srcdir)/SquareEffect.cs $(srcdir)/WaveyEffect.cs


EXTRA_DIST = 				\
	Program.cs \
    Borders.cs \
    EdgeEffect.cs \
    IEffect.cs \
    NoEffect.cs \
    SquareEffect.cs \
    WaveyEffect.cs
