## Roadmap ##
This is the current roadmap for Otis library. It contains features which are needed, as well as features which are fun. Some of them will probably never be implemented. It could also be changed at any time. Feel free to suggest anything you think might be missing, or boo anything that you consider wrong, broken, stupid or useless.

### Version 0.1 ###
  * Configuration via attributes and XML
  * XML sources: standalone file, embedded resource
  * Standard expression mapping
  * IList to/from Array
  * XML schema to support intellisense in Visual Studio
  * Custom configuration providers
  * Assembler generation:  in-memory, on the file system, source code only
  * Helper functions for custom conversions
  * Expression formatting
  * Null Value handling

### Version 0.2 ###
  * Aggregate functions
  * Simplified string expressions
  * Projections: constant -> constant (e.g. enum -> enum, enum->string, constant->enum, ...)

### Version 0.2.34.1 ###
  * this is a nightly build which fixes some bugs and adds some minor features
  * new aggregate function: concat
  * support for complex path items in aggregates: $Projects/Name -> $Projects/Name.Length
  * performance improvements
  * fixed: [issue #3](https://code.google.com/p/otis-lib/issues/detail?id=#3): automatically add assemblies for base types/interfaces (in version 0.2.17)
  * fixed: [issue #5](https://code.google.com/p/otis-lib/issues/detail?id=#5) - NullValue handling

### Version 0.2.68.1  (Available for download) ###
  * this is a nightly build which fixes some bugs
  * fixed: [issue #8](https://code.google.com/p/otis-lib/issues/detail?id=#8)
  * fixed: [issue #9](https://code.google.com/p/otis-lib/issues/detail?id=#9)
  * fixed: [issue #10](https://code.google.com/p/otis-lib/issues/detail?id=#10)

### Version 0.3 (in work) ###
  * Easier sub-component mapping (like "[component](component.md)" in NHibernate)
  * Factories, factory methods
  * Better default/null value handling (also for value types)
  * Preprocessing helper functions (like existing helpers, but called before transformation) (implemented as of 06.03.08)
  * Injectable Code Generators to implement Custom Assemblers
  * Injectable Namespace Name Provider
  * Injectable Assembler Name Providers
  * Performance improvements (ideally to 10% margin of hand written assemblers performance) (implemented as of 07.03.08, currently ~35% slower)

### Version 0.4 ###
  * NHibernate uninitialized collections and references
  * Events for hooking into conversion process
  * Inheritance issues
  * Better debugging support
  * Aggregate function parameters: [Map("concat[delimiter=', ']:$Documents/Name")]

### Version 0.5 ###
  * Move all strings to resources
  * Localization (e.g. for DateTime conversions)

### Future versions ###
  * Reverse (bidirectional) conversion
  * Tool for generation of XML mapping from attributes
  * Standalone assembly/source code generator (can be used as a part of an automated build)
  * Object->map transformation: user.FirstName -> userMap["FirstName"]
  * Filtering: [Map("$Projects/Tasks", Filter = "$Duration > 3"]
  * Statistics
  * Contexts
  * Type safe mapping via delegates: something like `[Map({(User u) => u.First + " " + u.Last})]` in C#3.0