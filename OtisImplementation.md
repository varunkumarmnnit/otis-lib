# Introduction #

This article describes the implementation of assembler types, and its repercussions on library performance.


# Details #
Otis **doesn't** rely on reflection to perform the transformations. There are two problems with that approach:
  * It is slow
  * Reflection is useless if assembly with types is obfuscated

Instead, Otis reads the mappings and generates a new assembly which implements a single assembler class, which implements `IAssembler<Target, Source>` interface for all mapped transformations. By default, this assembly is generated in the client application memory, but the library can also be instructed to generate it on the file system, so that it can be referenced from another assembly. Another options is to just generate the assembler source code, and build it as a part of different project.

This way, transformation performs similar to hand-written assembler types.

In future, generated assembler could provide some additional services, like statistics, logging, etc, but this is out of scope of the current release