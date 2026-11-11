Proyecto Ingeniería de Software Lila's Boutique

===

¿De que trata el proyecto?

Es el desarrollo de un sitio web para el correcto manejo de inventario para la tienda Lila's Boutique realizado en C#-Blazor y el entorno de desarrollo .NET



1. El problema que resuelve

Problemática actual: La tienda de ropa Lila’s Boutique (6 trabajadores) maneja su inventario de forma manual o en hojas de cálculo desarticuladas, lo que provoca descuadre

de stock, pérdida de ventas por falta de tallas/colores y falta de visibilidad del valor real del inventario disponible. Una aplicación 100% web centraliza la información, permitiendo a los vendedores consultar el stock desde el mostrador en tiempo real sin requerir instalaciones locales complejas, eliminando los tiempos de espera del cliente.





2\.  Funcionalidades del sistema



Asignación de roles: El sistema puede diferenciar 3 tipos de roles diferentes siendo el administrador, bodeguero y vendedor cada uno teniendo diferentes funciones y por lo tanto diferentes accesos al sistema de esta manera habiendo una clara estructura gerarjica donde el admin tendrá el nivel de permisos mas alto para poder arreglar errores cometidos por otros perfiles.



Crear de nuevas prendas y sus variantes: El admin es el único que podrá crear nuevos tipos y variantes de prenda a su vez de poder editarla y eliminarla de ser requerido. Tambien supervisar a los demás perfiles actividolos o desactivadolos y cambiando su contraseña según su criterio. El Tambien podrá ver las alertas que da e sistema sobre los productos que tienen un bajo nivel de stock para facilitar la toma de decisiones.



Visualización del inventario: El inventario mostrara todos las prendas junto con sus variantes en marca, categoria, talla y color. Para facilitar la navegacion de todo el inventario se implementa un filtro de busqueda de cada datos que esta presente en las prendas.



Registrar movimientos realizados en el inventario: Con las plantillas de las prendas se tiene que hacer movimientos de entradas y salidas respectivas. Las entradas de stock se deberán al reabastecimiento periódico que realiza la tienda y las salidas de stock serán dadas por las ventas de producto o mermas en el caso que presenten algún daño.





3\.  Tecnología utilizada



Para este proyecto se empleará Blazor para el front-end y C# para el back-end, respaldado por Entity Framework Core. Esta elección se ha tomado ya que se unifica el ecosistema de desarrollo bajo .NET, reduciendo la curva de desarrollo al uso previo de estas herramientas. Al contrastar esta arquitectura con alternativas populares como el stack MERN/MEAN (React o Angular para el cliente, Node.js y PostgreSQL para el servidor), se determinó que, aunque PostgreSQL ofrece sólidas características relacionales y Node.js posee un ecosistema extenso, su adopción obligaría a gestionar lenguajes inconexos entre sí aumentando la curva de aprendizaje y complicando un desarrollo optimo entre estas herramientas. La elección de Blazor/C# elimina la necesidad de frameworks basados en JavaScript como React o Angular, permitiendo reutilizar modelos y validaciones de forma nativa.



4\. Como ejecutar el programa de forma local



1\) Descargar los archivos del repositorio de GitHub.

2\) Entra a la carpeta del repositorio y elimina los archivos internos de las siguientes carpetas: .git, .vs, bin y obj.
3) Restaura la base de datos desde Microsoft SQl Server.
4) Abre la solución del programa en Visual Studio.

5\) Ubica el archivo appsetting.json y cambia la cadena de conexion por defecto por la de tu maquina actual.

6\) Compila y usa las siguientes las credenciales proporcionadas para acceder.

