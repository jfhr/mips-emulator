[![.NET](https://github.com/jfhr/mips-emulator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jfhr/mips-emulator/actions/workflows/dotnet.yml)

# mips-emulator

The goal of this project is to create a complete emulator of the MIPS runtime and assembler that runs in the browser. 

I am developing this purely as a study project, and there is no guarantee of completeness or accuracy. If you are looking for a productive MIPS emulator, you can probably find better free options.

## What is MIPS anyway?

The *Microprocessor without Interlocked Pipelined Stages*, or *MIPS* for short is an instruction set architecture. Essentially, it is an abstract model of a computer. A CPU that follows this model is called an implementation.

These days, most CPUs implement other architectures, such as x86 or ARM. In order to develop MIPS-compatible software on these systems, an emulator is useful. It simulates all essential components of a computer, including the arithmetic-logical unit, the registers, and the memory. Users can run their software in the emulator and examine their behavior.

## Other MIPS emulators already exist. Why write another one?

Several other such programs already exist, including some that run in the browser. It is not the goal of this project to replace any existing applications. Rather, it was created as a study project, and a proof-of-work of implementing such a project using WebAssembly, a relatively new technology.

## What programming languages and frameworks do you use?

So far, everything is implemented in C#, and will run in the browser using [WebAssembly](https://webassembly.org/) and [Blazor](https://blazor.net). [Bootstrap](https://getbootstrap.com/) is used for the look & feel.
