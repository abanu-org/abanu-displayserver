build: assembly binary

assembly:
	(cd src && ../../abanu/abctl tool msbuild /p:Configuration=Debug /m -Verbosity:minimal)

binary:
	../abanu/abctl build external $(shell ../abanu/abctl abspath $(abspath bin/Abanu.DisplayServer.exe))

