SUBDIRS = sources sources/glue doc samples

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = cogl-sharp-1.0.pc

EXTRA_DIST = cogl-sharp-1.0.pc.in
DISTCLEANFILES = cogl-sharp-1.0.pc

regenerate:
	bindinate --regenerate=true --merge-with=Cogl-2.0,CoglPango-1.0 --gir=Cogl-1.0

