### Description ###

_**Otis**_ is a .Net object transformation library, i.e. an object to object mapper.

It automatically generates converter assemblies or classes which convert instances of one type to instances of some other type. These transformation can be described in type metadata using attributes, or separately in an XML source (file, string, database)

Otis is intended to be used to solve some common design and implementation tasks, e.g. to ease the implementation of support for **[DTO](http://martinfowler.com/eaaCatalog/dataTransferObject.html)** classes (more [here](http://msdn2.microsoft.com/en-us/library/ms978717.aspx)), or to convert business domain type instances to presentation layer instances, but more generally, it can be used anywhere where a transformation between different types is needed.

Otis removes the need to manually implement type converters.

It doesn't use reflection in runtime (only during configuration phase), instead it compiles an in-memory converter assembly on-the-fly to improve the performance. As an option, it is possible to generate the assembly on the file system to include it in other project, or to generate the converter source code which can be compiled as a part of another project.

Otis is currently in an early alpha stage (version 0.2), but it already implements quite a few useful features. To check and discuss existing and future features, refer to [roadmap](OtisRoadmap.md).

The library is licensed under the [MIT license](http://www.opensource.org/licenses/mit-license.php), which essentially gives you the right to do whatever you want with it, except blaming the author if something goes wrong.

For more details see [Getting Started](GettingStarted.md) topic.