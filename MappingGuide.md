# Introduction #

A mapping in Otis is a relation between two types (classes or structs), which
specifies which values will be set to properties on the target type when a source tpye instance is mapped to a target type instance.

The new value is described by an expression, which doesn't have to reference source type members only. It can be any valid C# expression.

As stated in [Getting Started](GettingStarted.md), mappings can be defined either through XML files or through attributes on the target class. Additionaly to
the mapping expression, it is also possible to specify a helper function for the mapping. This function will be invoked after the defined member mappings are executed, to perform the custom transformation.

For every mapped property or field of the target class, it is possible to specify the null value replacement (expression assigned to target member if the source expression is null). Also, if the target member is a string, it is possible to specify the format string which will be applied to the source expression.

XML mappings are described in article [XML mapping definitions](XmlMappings.md).

Metadata mappings are described in article [Metadata mapping definitions](MetadataMappings.md).