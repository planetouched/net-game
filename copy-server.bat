rmdir /s/q "DedicatedServerUnity/Assets/Shared"
rmdir /s/q "DedicatedServerUnity/Assets/Server"
robocopy Assets/Shared DedicatedServerUnity/Assets/Shared /S /XF *.meta
robocopy Assets/Server DedicatedServerUnity/Assets/Server /S /XF *.meta