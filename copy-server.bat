rmdir /s/q "DedicatedServer/src/Shared"
rmdir /s/q "DedicatedServer/src/Server"
robocopy Assets/Shared DedicatedServer/src/Shared /S /XF *.meta
robocopy Assets/Server DedicatedServer/src/Server /S /XF *.meta