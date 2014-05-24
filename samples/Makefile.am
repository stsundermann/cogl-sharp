SUBDIRS = 

TARGETS = cogl-hello-world.exe

DEBUGS = $(addsuffix .mdb, $(TARGETS))
assemblies = 					\
	$(top_builddir)/out/cogl-sharp.dll

references=$(addprefix -r:, $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS) $(DEBUGS)

cogl-hello-world.exe: $(srcdir)/HelloWorld.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:cogl-hello-world.exe $(references) $(GTK_SHARP_LIBS) $(srcdir)/HelloWorld.cs

EXTRA_DIST = 				\
	HelloWorld.cs
