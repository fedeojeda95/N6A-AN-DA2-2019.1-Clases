# Mocking

Vamos a estudiar cómo podemos probar nuestro código evitando probar también sus dependencias, asegurándonos que los errores se restringen únicamente a la sección de código que efectivamente queremos probar. Para ello, utilizaremos una herramienta que nos permitirá crear Mocks. La herramienta será Moq.

## ¿Qué son los Mocks?

Los mocks son unos de los varios "test doubles" (es decir, objetos que no son reales respecto a nuestro dominio, y que se usan con finalidades de testing) que existen para probar nuestros sistemas. Los más conocidos son los Mocks y los Stubs, siendo la principal diferencia en ellos, el foco de lo que se está testeando.

Antes de hacer énfasis en tal diferencia, es importante aclarar que nos referiremos a la sección del sistema a probar como SUT (*System under test*). Los Mocks, nos permiten verificar la interacción del SUT con sus dependencias. Los Stubs, nos permiten verificar el estado de los objetos que se pasan. Como queremos testear el comportamiento de nuestro código, utilizaremos los primeros.

## Tipos de *Test Doubles*

Tipo | Descripción
------------ | -------------
**Dummy** | Son objetos que se pasan, pero nunca se usan. Por lo general, solo se utilizan para llenar listas de parámetros que tenemos que pasar si o si.
**Fake** | Son objetos funcionales, pero generalmente toman algún atajo que los hace inadecuados para la producción (una base de datos en la memoria es un buen ejemplo).
**Stubs** | Brindan respuestas predefinidas a las llamadas realizadas en el test, por lo general no responden a nada que no se use en el test.
**Spies** | Son Stubs pero que también registran cierta información cuando son invocados.
**Mocks** | Son objetos pre-programados con expectativas (son las llamadas que se espera que reciban). De todos estos objetos, los Mocks son los unicos que verifican el comportamiento. Los otros, solo verifican el estado.

## ¿Por qué los queremos usar?

Imaginense que estamos probando el modulo A. Este modulo A utiliza otro modulo, el modulo B. Hacemos nuestras pruebas unitarias, probando el comportamiento que es esperado. Dentro de nuestro grupo de pruebas, vemos que una falla y no sabemos porque. Resulta que el modulo B tiene un problema, un bug, el cual causa que nuestras pruebas del modulo A no pasen. Esto es un problema, nosotros queremos probar **solo** el modulo A, no el B.

Cuando hacemos pruebas unitarias, queremos probar objetos y la forma en que estos interactúan con otros objetos. Para ello creamos instancias de Mocks, es decir, objetos que simulen el comportamiento externo (es decir, la interfaz), de un cierto objeto. Son objetos tontos, que no dependen de nadie, siendo útiles para aislar una cierta parte de la aplicación que queramos probar. 

Hay ciertos casos en los que incluso los mocks son realmente la forma más adecuada de llevar a cabo pruebas unitarias. 

## Modificacion de nuestro codigo actual

Para poder hacer hacer mocks de las dependencias de un objeto que buscamos testear, es necesario cambiar el codigo que tenemos actualmente. Primero, analizaremos como es que se definen las dependencias en nuestro codigo. Tomemos una clase llamada *UserLogic* que estaría `Homeworks.BusinessLogic`. Esta clase depende de *UserRepository* de `Homeworks.DataAccess`. Como definimos esta dependencia? Simplemente en el constructor de *UserLogic* creamos una nueva instancia. 

```c#
    private UserRepository repository;

    public UserLogic() {
        repository = new UserRepository();
    }
```

El problema que tiene este enfoque es que cuando creamos una instancia de *UserLogic*, se creara una instancia **real** de *UserRepository*, y no podremos inyectarle un instancia mockeada del repository. La solución (por ahora) es agregar otro constructor que reciba el *UserRepository*. Sin embargo, esto no es suficiente: ya que no debemos recibir por parametro una instancia real del repositorio. Debemos crear una interfaz, la cual el repositorio implemente. 

```c#
    private IUserRepository repository;

    public UserLogic(IUserRepository repository = null) {
        if (repository == null) {
            this.repository = new UserRepository();
        } else {
            this.repository = repository;
        }
    }
```

La interfaz *IUserRepository* contendra los mismos metodos que tenia nuestro *UserRepository*. Esta tambien sera implementada por los mocks que crearemos mas adelante, lo cual nos permite poder crear un *UserLogic* con el mock! Debemos repetir esto para todas las clases que dependan de otra clase. Por ejemplo, UserController debera poder recibir en el constructor una interfaz que sea implementada *UserLogic* también.

Al hacer esto, no estamos atando la instancia a un objeto en particular, si no que estamos **inyectando** la dependencia. Veremos este concepto en particular mas adelante. Todo objeto que queramos mockear, debera tener una interfaz bien definida.

![alt text](http://tutorials.jenkov.com/images/java-unit-testing/testing-with-di-containers.png)

En consecuencia, generamos un **bajo acoplamiento** entre una clase y sus dependencias, lo cual nos facilita utilizar un framework de mocking. Especialmente para aquellos objetos que dependen de un recurso externo (una red, un archivo o una base de datos).

A pesar de que aun se esta instanciando la clase directamente en el constructor, veremos mas adelante como podemos remover eso. Iremos aun mas adelante y removeremos la instanciacion en el constructor. Por ahora, hacemos lo suficiente para que podamos hacer los tests.

Debemos hacer esto para todas las dependencias que tengamos.

## Empezando con Moq

## WebApi

Para comenzar a utilizar Moq, comenzaremos probando nuestro paquete de controllers de la WebApi. Para esto, crearemos un nuevo proyecto de MSTests (Homeworks.WebApi.Tests) y le instalamos Moq. Tambien instalamo `AspNetCore`, ya que es necesario para testear un controller.

```
dotnet new mstest -n Homeworks.WebApi.Tests
cd Homeworks.WebApi.Tests
dotnet add package Moq
dot net add package Microsoft.AspNetCore.App
```

Luego al proyecto de tests le agregaremos las referencias a WebApi, Domain y BusinessLogic.Interface

```
dotnet add reference ../Homeworks.WebApi
dotnet add reference ../Homeworks.Domain
dotnet add reference ../Homeworks.BusinessLogic
```

Una vez que estos pasos estén prontos, podemos comenzar a realizar nuestro primer test. Creamos entonces la clase `UsersControllerTests`, y en ella escribimos el primer `TestMethod`. 

## Probando el POST

```C#
[TestClass]
public class UsersControllerTests
{
    [TestMethod]
    public void CreateValidUserOkTest()
    {
        //Arrange
        
        //Act
        
        //Assert
    }

}
```

Para ello seguiremos la metodología **AAA: Arrange, Act, Assert**.

* **Arrange**: Contruimos el objeto mock y se lo pasamos al sistema a probar
* **Act**: Ejecutamos el sistema a probar
* **Assert**: Verificamos la interacción del *SUT* con el objeto mock.

```C#
[TestMethod]
public void CreateValidHomework()
{
    Homework homework = new Homework();
    homework.DueDate = DateTime.Now;
    homework.Description = "testing description"; // 1

    var mock = new Mock<IHomeworksLogic>(MockBehavior.Strict); // 2
    var controller = new HomeworksController(mock.Object);

    var result = controller.Post(homework); // 4
    var createdResult = result as CreatedAtRouteResult; // 5
    var model = createdResult.Value as Homework; // 6

    //Assert
}
```

Veremos que pasa en el test:

1) Crea un objeto de `Homework` que usaremos para el mock. Este retorna data que no nos importa, es solo para testing
2) Creamos el mock. La notacion es `new Mock<A>` siendo A la interfaz que queremos mockear. El parametro (`MockBehhavior.Strict`) es una parametro de configuracion del mock. `.Strict` hace que se tire una excepcion cuando se llama un metodo que no fue mockeado, mientras que `.Loose` retorna un valor por defecto si se llama un metodo no mockeado.
3) Se crea el controlador (`HomeworksController`) con el objeto mockeado.
4) Se ejecuta el metodo Post del controlador
5) Debido a que la clase retorna un `CreatedAtRouteResult`, como se puede ver en la implementacion, casteamos el resultado a esto 
6) Mediante `.Value` obtenemos el resultado de la request

Sin embargo, nos falta definir el comportamiento que debe tener el mock del nuestro `IHomeworksLogic`. Esto es lo que llamamos **expectativas** y lo que vamos asegurarnos que se cumpla al final de la prueba. Recordemos, los mocks simulan el comportamiento de nuestros objetos, siendo ese comportamiento lo que vamos a especificar a partir de expectativas. Para ello, usamos el método **Setup**.

### ¿Cómo saber qué expectativas asignar?

Esto va en función del método de prueba. Las expectativas se corresponden al caso de uso particular que estamos probando dentro de nuestro método de prueba. Si esperamos probar el `Post()` de nuestro `HomeworksController`, y queremos mockear la clase `HomeworksLogic`, entonces las expectativas se corresponden a las llamadas que hace `HomeworksController` sobre `HomeworksLogic`. 

Veamos el método a probar, el `POST` de un usuario:

```C#
[HttpPost]
public IActionResult Post([FromBody] Homework homework)
{
    try {
        Homework createdHomework = homeworksLogic.Create(homework);
        return CreatedAtRoute("Get", new { id = homework.Id }, createdHomework);
    } catch(ArgumentException e) {
        return BadRequest(e.Message);
    }
}
```

La línea que queremos mockear es la de:

```C#
Homework createdHomework = homeworksLogic.Create(homework);
``` 

Entonces:
1) Primero vamos a decirle que esperamos que sobre nuestro Mock que se llame a la función Create().
2) Luego vamos a indicarle que esperamos que tal función retorne un user que definimos en otro lado.

```C#
[TestMethod]
public void CreateValidHomework()
{
    Homework homework = new Homework();
    homework.DueDate = DateTime.Now;
    homework.Description = "testing description";

    var mock = new Mock<IHomeworksLogic>(MockBehavior.Strict);
    mock.Setup(m => m.Create(It.IsAny<Homework>())).Returns(homework); 
    var controller = new HomeworksController(mock.Object);

    var result = controller.Post(homework);
    var createdResult = result as CreatedAtRouteResult;
    var model = createdResult.Value as Homework;

    mock.VerifyAll();
    Assert.AreEqual(homework.Description, model.Description);
    Assert.AreEqual(homework.DueDate, model.DueDate);
}
```

Veamos que le agregamos al metodo de test:

1) Seteamos el mock. Cuando decimos setear, queremos decir que le definimos el comportamiento que queremos en el test de un metodo de un mock. El metodo setup recibe una funcion inline de LINQ, la cual recibe el objeto a mockear. Es decir, en este caso `m` es un objeto de tipo `IHomeworksLogic`. Aqui estamos definiendo que para el metodo `Create`, cuando reciba cualquier parametro de tipo `Homework` (`It.IsAny<Homework>()`). En este caso, se retorna el `homework`.
2) También debemos verificar que se hicieron las llamadas pertinentes. Para esto usamos el método `VerifyAll` del mock. Este revisa que fueron llamadas todas las funciones que mockeamos. 
3) Verificamos que los datos obtenidos sean correctos. Para esto hacemos asserts (aquí estamos probando estado) para ver que los objetos usados son consistentes de acuerdo al resultado esperado.

Corremos los tests utilizando `dotnet test` y vemos que nuestro test pasa 😎!

## Mockeando excepciones

Ahora veamos como probar otros casos particulares, por ejemplo cuando nuestro `Post()` del Controller nos devuelve una **BadRequest**.

Particularmente, en el caso que hemos visto antes nuestro Controller retornaba `CreatedAtRoute` para dicha situación. Ahora, nos interesa probar el caso en el que nuestro Controller retorna una BadRequest. Particularmente, esto se da cuando el método `Create()` recibe `null`. Para probar este caso entonces, seteamos dichas expectativas y probemos.

```C#
[TestMethod]
public void CreateInvalidHomeworkBadRequestTest()
{
    var mock = new Mock<IHomeworksLogic>(MockBehavior.Strict);
    mock.Setup(m => m.Create(null)).Throws(new ArgumentException());
    var controller = new HomeworksController(mock.Object);

    var result = controller.Post(null);

    mock.VerifyAll();
    Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
}
```

Lo que hicimos fue indicar que cuando se invoque `Create` con el parametro en `null`, se lance `ArgumentException`. En consecuencia, cuando nuestro controller llame a este mock, se lanzara `ArgumentException` causando que nuestro controller la capture y retorne `BadRequest`.

Finalmente entonces, verificamos que las expectativas se hayan cumplido (con el `VerifyAll()`), y luego que el resultado obtenido sea un `BadRequestObjectResult`, usando el metodo de `Assert` `IsInstanceOfType`.

## BusinessLogic

Creamos nuestro proyecto:

```
dotnet new mstest -n Homeworks.BusinessLogic.Tests
cd Homeworks.WebApi.BusinessLogic
dotnet add package Moq
```

Agregamos las referencias a `BusinessLogic`, `Domain` y finalmente a `DataAccess`

Creamos entonces la clase UserLogicTests. 

## Probando el Create User

Entonces:
1) Primero vamos a decirle que esperamos que sobre nuestro Mock que se llame a la función Add().
2) Luego vamos a indicarle que esperamos que se llame la función Save().
3) Invocamos Create
4) Verificamos que se hicieron las llamadas pertinentes, y realizamos Asserts

```C#
[TestMethod]
public void CreateValidHomeworkTest()
{
    Homework homework = new Homework();
    homework.DueDate = DateTime.Now;
    homework.Description = "testing description";

    var mock = new Mock<IHomeworksRepository>(MockBehavior.Strict);
    mock.Setup(m => m.Add(It.IsAny<Homework>()));
    mock.Setup(m => m.Save());

    var homeworksLogic = new HomeworksLogic(mock.Object);

    var result = homeworksLogic.Create(homework);

    mock.VerifyAll();
    Assert.AreEqual(homework.Description, result.Description);
    Assert.AreEqual(homework.DueDate, result.DueDate);
}
```

## Ejercicio

* Agregar muchos mas tests a todo el sistema!

# Mas Info

En la documentación de MOQ, se encuentran varios ejemplos y definiciones de como hacer los mocks:

* [MOQ](https://github.com/moq/moq4)
* [MOQ quickstart](https://github.com/Moq/moq4/wiki/Quickstart)

Tipos de respuesta de Web API. Puede ser importante saberlo para testear las respuestas de los controladores de Web API

* [Action return types](https://docs.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-2.2)

Info en general:

* [Mocks Aren't Stubs](https://martinfowler.com/articles/mocksArentStubs.html)
* [Asserting exception with MSTest](http://www.bradoncode.com/blog/2012/01/asserting-exceptions-in-mstest-with.html)
* [Exploring assertions (recomendado)](https://www.meziantou.net/2018/01/22/mstest-v2-setup-a-test-project-and-run-tests)
