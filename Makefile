CUR_DIR = $(CURDIR)

all: net bin

.PHONY: all net bin

bin: net ../abanu/os/Abanu.DisplayServer.bin

../abanu/os/Abanu.DisplayServer.bin:
	cd $(CUR_DIR)
	../abanu/abctl build external $(shell ../abanu/abctl abspath $(abspath bin/Abanu.DisplayServer.exe))

net: bin/Abanu.DisplayServer.exe

bin/Abanu.DisplayServer.exe:
	cd $(CUR_DIR)
	(cd src && ../../abanu/abctl tool msbuild /p:Configuration=Debug /m -Verbosity:minimal)
