# mips-emulator

Ziel dieses Projekts ist es, einen vollständigen Emulator der MIPS-Runtime und des Assemblers zu erstellen, der im Browser läuft. 

Ich entwickle dies als reines Studienprojekt, und es gibt keine Garantie auf Vollständigkeit oder Korrektheit. Wenn Sie nach einem produktiven MIPS-Emulator suchen, können Sie sicherlich bessere kostenlose Optionen finden.

## Was ist MIPS überhaupt?

Der *Microprocessor without Interlocked Pipelined Stages*, oder kurz *MIPS*, ist eine Befehlssatzarchitektur, also ein abstraktes Modell eines Computers. Eine CPU, die diesem Modell folgt, wird als Implementierung bezeichnet.

Heutzutage implementieren die meisten CPUs andere Architekturen, wie z.B. x86 oder ARM. Um auf diesen Systemen MIPS-kompatible Software zu entwickeln, ist ein Emulator nützlich. Er simuliert alle wesentlichen Komponenten eines Computers, einschließlich der arithmetisch-logischen Einheit, der Register und des Speichers. Die Benutzer können ihre Software im Emulator ausführen und nachvollziehen.

## Andere MIPS-Emulatoren existieren bereits. Warum einen weiteren schreiben?

Es gibt bereits mehrere andere solcher Programme, einschließlich einiger, die im Browser laufen. Es ist nicht das Ziel dieses Projekts, irgendwelche bestehenden Anwendungen zu ersetzen. Vielmehr wurde es als Studienprojekt und als Nachweis für die Implementierung eines solchen Projekts unter Verwendung von WebAssembly, einer relativ neuen Technologie, erstellt.

## Welche Programmiersprachen und Frameworks verwendest du?

Bis jetzt ist alles in C# implementiert und wird im Browser mit [WebAssembly](https://webassembly.org/) und [Blazor](https://blazor.net) ausgeführt. Für das Look & Feel wird [Bootstrap](https://getbootstrap.com/) verwendet.