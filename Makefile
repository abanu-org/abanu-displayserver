.PHONY: compile

compile:
	../abanu/abctl build external $(shell ../abanu/abctl abspath $(abspath bin/Abanu.DisplayServer.exe))


