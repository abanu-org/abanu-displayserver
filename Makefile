all: net bin

net:
	(cd src && ../../abanu/abctl tool msbuild /p:Configuration=Debug /m -Verbosity:minimal)

bin:
	../abanu/abctl build external $(shell ../abanu/abctl abspath $(abspath bin/Abanu.DisplayServer.exe))

