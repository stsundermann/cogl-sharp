lib_LTLIBRARIES = libcoglsharpglue-1.16.2.la

libcoglsharpglue_1_16_2_la_SOURCES =

nodist_libcoglsharpglue_1_16_2_la_SOURCES = generated.c

libcoglsharpglue_1_16_2_la_LDFLAGS = -module -avoid-version -no-undefined

libcoglsharpglue_1_16_2_la_LIBADD = $(COGL_LIBS)

INCLUDES = $(COGL_CFLAGS) $(PANGO_CFLAGS) $(CAIRO_CFLAGS) -I$(top_srcdir)

libcoglsharpglue.dll: $(libcoglsharpglue_1_16_2_la_OBJECTS) libcoglsharpglue.rc libcoglsharpglue.def
	./build-dll libcoglsharpglue $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST =
