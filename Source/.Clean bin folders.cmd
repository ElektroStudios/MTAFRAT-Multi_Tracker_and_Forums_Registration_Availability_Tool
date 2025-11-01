@ECHO OFF

:: Debug folder
(
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\cs"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\de"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\fr"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\it"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\ja"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\ko"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\pl"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\pt-BR"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\ru"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\tr"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\zh-Hans"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\zh-Hant"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\runtimes\linux"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\runtimes\osx"
	RD /Q /S ".\MTAFRAT\bin\Debug\net8.0-windows\cache"
)>NUL 2>&1
ECHO:Debug dolder has been cleaned.

:: Release folder
(
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\cs"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\de"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\fr"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\it"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\ja"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\ko"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\pl"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\pt-BR"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\ru"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\tr"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\zh-Hans"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\zh-Hant"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\runtimes\linux"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\runtimes\osx"
	RD /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\cache"

	DEL /Q /S ".\MTAFRAT\bin\Release\net8.0-windows\MTAFRAT.pdb"
)>NUL 2>&1
ECHO:Release dolder has been cleaned.

PAUSE
EXIT