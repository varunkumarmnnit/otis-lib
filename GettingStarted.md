# Introduction #

This article describes how to get going with Otis. It shows how to configure mappings between types and how to perform the transformations

# Basic idea #
When implementing code for transformation between the types, the required effort can make significant part of overall effort on project, thus making the project more expensive and/or delaying release date. Types which implement these transformations are usually called assemblers, and often consist of heaps of trivial code which has to be properly tested and maintained. The idea behind otis is that the assembler requirements should be expressed declaratively, and the library will take care of generating the assembler types.

# Pros & cons #
Like any other tool/practice, this is not a silver bullet.

Main benefit of declarative assembler is that it reduces the effort needed for writing and maintenance of mapping code (although I am sure that some people will disagree). Otis also supports aggregate functions which can traverse an object graph and calculate or collect needed data. For really hairy problems, you can register a helper function to perform custom processing which is still easier than writing a whole new assembler.

Disadvantages? In my opinion, the biggest one is lack of type safety due to text expressions being used to define the mapping between types. Otis will try to help you with diagnostic messages, but of course it would be better if compiler could find the error immediately. However, short of this, the best defense is to write a single unit test which tries to build an assembler. If it fails, mappings are invalid, and hopefully, an error message will show you what is wrong.

# Setup #
Otis setup consists of two tasks (and coresponding two lines of code):
  * Instantiating the new Otis.Configuration object
  * Telling it to configure itself from some source

What is left to do is to retrieve the required assembler from the configuration object, and start assembling the objects. Here is the snippet:
```
    // configure the new Configuration object using metadata of types in the current assembly
    Configuration cfg = new Configuration();            // instantiate a new Configuration, one per application is needed
    cfg.AddAssembly(Assembly.GetExecutingAssembly());   // initialize it

    // retrieve the assembler and transform the source object
    IAssembler<UserDTO, UserEntity> asm = cfg.GetAssembler<UserDTO, UserEntity>();                  // retrieve the assembler
        
    UserEntity entity = ...                             // retrieve a UserEntity instance from somewhere
    UserDTO dto = asm.AssembleFrom(entity);             // do the transformation
```

# Configuring the mappings #

There is more than one way to configure mappings
  * Add metadata to types using attributes
  * Setup transformations in a xml file
  * Custom mapping providers

This article only gives an overview of the mapping definitions. For more details, refer to MappingGuide.

## Configuration via metadata ##
To define transformations via type metadata all you have to do is mark target types and their members with appropriate attributes. To specify that a type is a target type of transformation, mark it with `[MapClass]` attribute. Then, for every member which will be mapped to source type, specify `[Map]` attribute. In following example, `UserDTO` is a target type for `UserEntity->UserDTO` transformation.

### Sample ###
Here is a complete sample of defining the mappings via attributes (for version 0.2), configuring the assembler and transforming the object:

```
// source class for conversion
class UserEntity
{
    public int Id { get ... }
    public string FirstName { get ... }
    public string LastName { get ... }
    public DateTime BirthDate { get ... }
    public IList<ProjectEntity> Projects { get ... }
    public UserEntity Boss { get ... }  
    public Gender Gender { get ... }  // Gender is an enum with values Male, Female
}

// target type for conversion
[MapClass(typeof(UserEntity))] // defines conversion from UserEntity
class UserDTO
{
    [Map]  // maps to the property of same name   
    public int Id { get ... }

    [Map("$UserName.ToLower()")]
    public string UserName { get ... }

    // expression enclosed in []: treats ' as " so you don't have to write "$FirstName + \" \" + $LastName"
    [Map("[$FirstName + ' ' + $LastName]")] 
    public string FullName { get ... }

    // Projection: map enum values to strings, e.g. Gender.Female => "Mrs."
    [Map("$Gender", Projection = "Gender.Male => Mr.; Gender.Female => Mrs.")]
    public string Title { get ... }

    [Map("$BirthDate", Format="Born on {0:D}")] // converts DateTime to string, and formats it as long date
    public string Birthday { get ... }
    
    [Map] // source is an IList<ProjectEntity>, target is array
    public ProjectDTO[] Projects { get ... }  
    
    [Map("$Projects.Count")]
    public int ProjectCount { get...} 

    // iterates over all tasks in all projects and calculates average tasks duration
    [Map("avg:$Projects/Tasks/Duration")]
    public double AvgTaskDuration { get...} 

    // iterates over all tasks in all projects and returns maximum task duration
    [Map("max:$Projects/Tasks/Duration")]
    public int MaxTaskDuration { get...} 
    
    [Map] // recursively maps the Boss property
    public UserDTO Boss { get ... } 
}

// usage
[Test]
public void Test()
{
    Configuration cfg = new Configuration();            // instantiate a new Configuration, one per application is needed
    cfg.AddType(typeof(UserDTO));                       // initialize it using type metadata, but easier is 
                                                        // cfg.AddAssembly(Assembly.GetExecutingAssembly()) to register all types at once
    IAssembler<UserDTO, UserEntity> asm                 // retrieve the assembler
        = cfg.GetAssembler<UserDTO, UserEntity>();
    UserEntity entity = ...                             // retrieve a UserEntity instance from somewhere
    UserDTO dto = asm.AssembleFrom(entity);             // do the transformation
    
    Assert.AreEqual(dto.Id,              entity.Id);
    Assert.AreEqual(dto.UserName,        entity.UserName.ToLower());
    Assert.AreEqual(dto.FullName,        entity.FirstName + " " + entity.LastName);
    Assert.AreEqual(dto.Title,           entity.Gender == Gender.Female ? "Mrs." : "Mr.");
    Assert.AreEqual(dto.Birthday,        entity.BirthDate.ToString("Born on {0:D}"));
    Assert.AreEqual(dto.Projects.Length, entity.Projects.Count);
    Assert.AreEqual(dto.ProjectCount,    entity.Projects.Count); 
      
    Assert.AreEqual(dto.Boss.Id,         entity.Boss.Id);
    Assert.AreEqual(dto.Boss.UserName,   entity.Boss.UserName.ToLower());
    Assert.AreEqual(dto.Boss.FullName,   entity.Boss.FirstName + " " + entity.LastName);
    
    // calculating max and avg task duration to test the transformation
    int max, cnt, sum;
    
    foreach(Project project in entity.Projects)
        foreach(Task task in project.Tasks)
        {
            cnt++;
            sum = sum + task.Duration; 
            max = task.Duration > max ? task.Duration : max;    
        }    
    double avg = (double)sum / cnt; 
        
    Assert.AreEqual(avg, dto.AvgTaskDuration);
    Assert.AreEqual(max, dto.MaxTaskDuration);        
}
```

## Configuration via XML files ##
Configuration via XML files makes source code cleaner, and is the preferred way to declare the mappings. These configuration files can be standalone files which are deployed with the application, or built into the application as resources.
If you choose this path, target class will look simply like this:

```
// target type for conversion
class UserDTO
{
    public int Id { get ... }
    public string UserName { get ... }
    public string FullName { get ... }
    public string Title { get ... }
    public string Birthday { get ... }
    public ProjectDTO[] Projects { get ... }  
    public int ProjectCount { get...} 
    public double AvgTaskDuration { get...} 
    public int MaxTaskDuration { get...} 
    public UserDTO Boss { get ... } 
}
```

The xml mapping looks like this:
```
<?xml version="1.0" encoding="utf-8" ?>
<otis-mapping xmlns="urn:otis-mapping-1.0">

    <class name="Otis.Sample.UserDTO, Otis.Sample" source="Otis.Sample.Domain.User, Otis.Sample" >
        <member name="Id" />  <!-- maps to the member with the same name -->
        <member name="UserName" /> 
        <member name="Boss" /> 
        <member name="FullName" expression="[$FirstName + ' ' + $LastName]" /> <!-- uses [] to avoid "$FirstName + &quot; &quot; + $LastName" -->
        <member name="Title" expression="$Gender" >
            <map from="Gender.Male" to="Mr." />     <!-- projections -->
            <map from="Gender.Female" to="Mrs." />
        </member> 
        <member name="Birthday"  expression="$BirthDate" format="Born on {0:D}"/>
        <member name="ProjectCount" expression="$Projects.Count" />
        <member name="AvgTaskDuration" expression="avg:$Projects/Tasks/Duration" />
        <member name="MaxTaskDuration" expression="max:$Projects/Tasks/Duration" />
    </class>
</otis-mapping>
```

Schema for mapping file is available as part of the archive which can be downloaded from this site. It can be copied into xml\schemas folder under the Visual Studio 2005 installation directory, to provide syntax checking in the VS editor. Of course, the mapping file is checked against the schema during the configuration.

If you chose to define mappings as XML resources, client code has to be slightly modified. Instead of
```
    cfg.AddAssembly(Assembly.GetExecutingAssembly());
```
which initializes the mapper using attributes on mapped types, you must call
```
    cfg.AddAssemblyResources(Assembly.GetExecutingAssembly(), "otis.xml"); 
```
This will tell mapper to read mappings from all assembly resource files which end with "otis.xml".

# See Also #
[IAssembler<Target, Source> interface](AssemblerInterface.md)

[Type mapping in detail](MappingGuide.md)

[Otis implementation and performance](OtisImplementation.md)

[Custom mapping providers](CustomProviders.md)