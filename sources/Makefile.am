RAW_API = $(ASSEMBLY_NAME)-api.raw
API = $(ASSEMBLY_NAME)-api.xml
METADATA = $(ASSEMBLY_NAME).metadata
DLL = $(top_srcdir)/out/$(ASSEMBLY_NAME).dll
DLLMAP = $(top_srcdir)/out/$(ASSEMBLY_NAME).dll.config
GLUEDIR = $(srcdir)/glue
COGL_GAPI_PARAMS = --generate $(srcdir)/$(API)

if COGLGSTREAMER
COGL_GAPI_PARAMS += --generate $(coglgst_api)
endif


gapidir = $(GAPIDIR)
gapi_DATA = $(API) $(coglgst_api)

# Add any extra source files you need here
sources = custom/*.cs

build_sources = AssemblyInfo.cs $(sources)

CLEANFILES = $(DLL) generated-stamp generated/*/*.cs $(API)

DISTCLEANFILES = AssemblyInfo.cs $(DLLMAP)

noinst_DATA = $(DLL)

EXTRA_DIST = \
	$(RAW_API) \
	$(sources) \
	$(METADATA) \
	$(coglgst_metadata) \
	AssemblyInfo.cs.in \
	$(ASSEMBLY_NAME).snk


all: generated-stamp $(ASSEMBLY)

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(GAPI_FIXUP) --api=$(srcdir)/$(API) --metadata=$(srcdir)/$(METADATA)

$(coglgst_api): $(srcdir)/$(coglgst_api_raw) $(srcdir)/$(coglgst_metadata)
	cp $(srcdir)/$(coglgst_api_raw) $(coglgst_api)
	chmod u+w $(coglgst_api)
	$(GAPI_FIXUP) --api=$(srcdir)/$(coglgst_api) --metadata=$(srcdir)/$(coglgst_metadata)

generated-stamp: $(API) $(coglgst_api)
	rm -rf generated/* && \
	$(GAPI_CODEGEN) $(COGL_GAPI_PARAMS) $(GTK_SHARP_CFLAGS) $(GSTREAMERSHARP_CFLAGS) \
		--outdir=generated \
		--glue-filename=$(GLUEDIR)/generated.c --gluelib-name=libcoglsharpglue-1.18.0.so \
		--glue-includes=cogl/cogl.h,cogl-pango/cogl-pango.h, --gapidir=$(gapidir) \
	 	--assembly-name=$(ASSEMBLY_NAME) && touch generated-stamp

$(DLL): $(build_sources) generated-stamp
	$(CSC) -nowarn:169 -unsafe -target:library $(GTK_SHARP_LIBS) $(GSTREAMERSHARP_LIBS) \
		$(build_sources)  generated/**/*.cs -out:$(DLL)

install-data-local:
	echo "$(GACUTIL) /i $(DLL) /f $(GACUTIL_FLAGS)";  \
        $(GACUTIL) /i $(DLL) /f $(GACUTIL_FLAGS) || exit 1;

uninstall-local:
	echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)"; \
        $(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;
