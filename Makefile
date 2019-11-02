.PHONY: compile

compile:
	(cd src && ../../abanu/abctl tool msbuild /p:Configuration=Debug /m -Verbosity:minimal)
	../abanu/abctl build external $(shell ../abanu/abctl abspath $(abspath bin/Abanu.DisplayServer.exe))


