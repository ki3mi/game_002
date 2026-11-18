# Mecánicas de Luz de vereda

La premisa, los objetivos educativos y los ejemplos de nivel están en el [documento de contexto](Context.md).

## Ciclo de juego y economía

1. **Inspeccionar.** El encargo muestra los servicios obligatorios, sus horarios, el presupuesto y el objetivo de excelencia. El jugador recorre el lugar y puede adelantar la hora para observar cómo se mueve la sombra.
2. **Construir y verificar.** Arma los recintos necesarios con bloques encajables, ubica los equipos y traza el cableado. Entra en primera persona al conectar, configurar o verificar el equipo. La interfaz actualiza el costo en créditos según las piezas, los equipos, el cableado y, cuando corresponde, los postes y el transporte. Cada encargo tiene su propio presupuesto.
3. **Probar.** La simulación avanza por las franjas del escenario. La generación cubre primero el consumo del momento. El excedente carga la batería y la energía almacenada cubre los déficits mientras quede reserva. Si se interrumpe un servicio obligatorio, el informe indica la hora y el motivo.
4. **Ajustar o entregar.** El jugador puede mover, sustituir y retirar piezas, y repetir la prueba cuantas veces quiera. Cada intento se evalúa por el costo de su diseño final. Una estrella basta para avanzar y se puede volver a mejorar la solución.

### Calificación

| Resultado | Condición |
| --- | --- |
| **Nivel incompleto** | Incumple al menos un requisito obligatorio del encargo. |
| **★** | Cumple todos los requisitos obligatorios y supera el presupuesto. |
| **★★** | Cumple todos los requisitos obligatorios dentro del presupuesto, con el objetivo de excelencia pendiente. |
| **★★★** | Cumple todos los requisitos obligatorios y el objetivo de excelencia dentro del presupuesto. Representa una instalación perfecta para ese encargo. |

El presupuesto admite un costo igual a su límite. Un encargo puede pedir como objetivo de excelencia terminar con una reserva mínima de batería o mantener el servicio durante una prueba adicional de clima adverso.

### Créditos de campaña e ingresos diarios

La campaña utiliza la misma unidad, **créditos**, para dos cifras que aparecen por separado. El **presupuesto del encargo** es la asignación del programa municipal con la que se evalúa el costo de una instalación. El **saldo de campaña** acumula los aportes de operación y se gasta en comprar mejoras del árbol de habilidades. Las estrellas se calculan con el presupuesto asignado al encargo.

Cada establecimiento incorporado al programa con al menos una estrella genera un aporte base de operación, definido por su nivel y tamaño. Al entregar por primera vez un encargo válido, termina el **día de campaña** y se suman los aportes de todos los establecimientos en servicio, incluido el recién incorporado. Después se aplican los potenciadores obtenidos por instalaciones destacadas. La jornada que se reproduce al pulsar **Probar** pertenece a la simulación del encargo. Repetir una prueba o volver a entregar un nivel anterior no adelanta el día de campaña ni añade otro aporte para el mismo establecimiento.

Las prácticas guiadas y la entrega de encargos abren nuevos tipos de establecimientos y situaciones. El jugador empieza con instalaciones escolares y avanza a trabajos que incorporan servicios continuos, como la refrigeración de un puesto de salud, o cargas flexibles, como una estación de bombeo con tanque. Los encargos de mayor escala ofrecen presupuestos de obra más amplios. A medida que el jugador incorpora establecimientos, crecen sus ingresos diarios. Las prácticas que habilitan encargos obligatorios se completan sin gastar créditos, de modo que el orden de compra de mejoras queda a elección del jugador.

### Desafío final de ciudad

El encargo final combina varios sectores urbanos en una red de generación renovable, distribución y almacenamiento. El jugador prueba el suministro en un día normal y en un escenario de baja generación. El informe muestra qué sectores cumplieron sus horarios y qué servicios críticos sufrieron interrupciones. La campaña se completa cuando la red cubre los requisitos de todos los sectores en ambas pruebas. El presupuesto y el objetivo de excelencia se evalúan con el sistema de estrellas de los demás encargos.

## Cámara e interacción

- **Exploración y planificación.** La cámara empieza en tercera persona. El jugador puede alternar manualmente entre tercera y primera persona mientras recorre el establecimiento, inspecciona el entorno o decide dónde ubicar los equipos.
- **Instalación, configuración y verificación.** Al iniciar una intervención, el juego cambia automáticamente a primera persona y mantiene esa vista mientras el jugador manipula cables, bornes, controles o instrumentos. En esta vista se muestran las manos, la herramienta seleccionada y el punto de trabajo.
- **Regreso a la exploración.** Al terminar o cancelar la intervención, se recupera la vista que el jugador tenía antes de iniciarla.

## Construcción modular

El jugador construye pieza por pieza en el escenario. La primera base fija la posición, altura y orientación de un módulo. Las bases contiguas, paredes y techos se alinean mediante puntos de encaje. La referencia es el montaje de piezas por encaje de *Rust*. [Diario de desarrollo de Facepunch](https://rust.facepunch.com/news/devblog-187)

### Colocación y edición

1. **Elegir la pieza.** El jugador abre el menú **Construir** desde su vista actual y selecciona un bloque. Ve su costo y el total que tendría el proyecto si lo colocara.
2. **Apuntar y orientar.** Una silueta translúcida sigue la mira. La primera base se previsualiza sobre el terreno y gira en pasos de 90°. Para las demás piezas, al apuntar cerca de un borde o soporte se resalta el encaje compatible. Si hay varios encajes, el jugador puede recorrerlos antes de confirmar.
3. **Validar y colocar.** La silueta y un mensaje indican si se puede colocar la pieza. El juego comprueba que esté dentro del área autorizada, que tenga apoyo y que su volumen no invada terreno, piezas u obstáculos, salvo en sus uniones previstas. Una base exige suelo firme que cubra su huella y una pendiente admisible para el nivel. Superar el presupuesto permite confirmar y afecta la calificación según las reglas de estrellas.
4. **Revisar el diseño.** El jugador puede retirar y recolocar piezas antes de entregar. Retirar una pieza descuenta su costo del diseño. Si sostiene otros bloques o tiene equipos o cables fijados, el juego resalta las dependencias y pide retirarlas o reubicarlas primero. La manipulación de conexiones y equipos mantiene el cambio automático a primera persona.

### Bloques iniciales y reglas de encaje

| Bloque | Dónde encaja | Función |
| --- | --- | --- |
| Base cuadrada | En suelo válido. Puede alinearse con el borde de otra base si dispone de apoyo propio. | Forma el piso del recinto y soporta equipos de suelo. |
| Pared completa | En un borde libre de la base. | Cierra un lateral y ofrece puntos para montar equipos y pasar cables. |
| Pared con paso de cable | En un borde libre de la base. | Cierra un lateral y habilita la transición entre un tramo exterior y uno oculto. |
| Marco de puerta | En un borde libre de la base con espacio de acceso. | Forma la entrada y admite una puerta. |
| Puerta | Dentro de un marco compatible, con espacio para abrirse. | Cierra el recinto y se puede abrir para inspeccionar los equipos. |
| Techo | Sobre paredes o marcos. Cada módulo de techo necesita apoyo en dos lados opuestos. | Cubre una base y protege los equipos que están debajo. |

El apoyo se comprueba siguiendo los encajes hasta las bases. El jugador puede dejar un recinto en obra mientras diseña. Para superar un nivel que exige resguardo, la prueba comprueba que las bases del recinto estén contiguas, que su perímetro esté cerrado por paredes y puertas, que cada base esté cubierta y que exista una entrada accesible. Los equipos que el encargo pide proteger deben estar dentro. El informe señala los requisitos de construcción pendientes junto con los eléctricos.

En *La escuela en la ladera*, el jugador puede montar un recinto compacto cerca del primer poste o ampliar la base para alojar más equipos. La orientación de la puerta determina desde dónde se inspecciona el tablero, y la ubicación del paso de cable cambia la longitud del tendido hasta el poste.

## Cableado y centrales alejadas

El jugador traza la ruta entre la fuente, el tablero y los equipos que debe alimentar. El cableado oculto dentro de paredes, techos y suelos se representa únicamente con **líneas punteadas** que indican por dónde pasa. En exteriores, el jugador puede tender **cables visibles suspendidos entre postes eléctricos**. La longitud del recorrido y la cantidad de postes afectan el costo del diseño.

En ciertos niveles, la fuente renovable se instala lejos del establecimiento. El jugador debe construir un **recinto de protección para los equipos de la central**, como el tablero y las baterías, y tender la conexión desde allí hasta el establecimiento. En esos encargos, completar el recinto y conectar ambas ubicaciones son requisitos obligatorios. La construcción, los postes y el cableado cuentan en el presupuesto del nivel.

### Familias de cables y desbloqueo

| Familia | Cuándo se incorpora | Aplicación en el juego |
| --- | --- | --- |
| Fotovoltaico | En la primera práctica con paneles solares. | Conecta los paneles con los equipos de la central. |
| Para canalización interior | Al aprender a distribuir energía desde el tablero. | Alimenta luminarias y otros equipos por rutas ocultas en paredes, techos o suelos. |
| Para tendido aéreo | Antes del primer encargo con una central alejada. | Conecta el recinto con el establecimiento mediante un tramo suspendido entre postes. |

Un mismo encargo puede introducir dos familias si el montaje requiere ambas. Cada familia se habilita después de una práctica con su recorrido y sus conexiones. En niveles posteriores se pueden introducir variantes de mayor capacidad para alimentar más equipos en un mismo tramo.

### Cortocircuitos y diagnóstico

El jugador debe ordenar y proteger los cables y revisar sus conexiones antes de probar la instalación. En la simulación, un cortocircuito puede ocurrir si dos conductores expuestos que deben permanecer separados entran en contacto, por ejemplo en un borne mal montado o en un tramo con aislamiento dañado. El cruce visual de dos rutas en el mapa puede darse con ambos cables aislados.

Durante la prueba, un cortocircuito activa la protección del circuito afectado e interrumpe el servicio que alimenta. El informe marca el tramo sospechoso. El jugador vuelve a la instalación desenergizada, inspecciona o verifica el tramo en primera persona, corrige el montaje y repite la prueba. Si el corte impide prestar un servicio obligatorio, el nivel queda incompleto.

### Descargas y guardado automático

Si el jugador manipula un cable o borne bajo tensión durante una corrección, recibe una descarga y reaparece en el último punto de guardado automático cercano a la zona de trabajo. El guardado registra el diseño, el estado del circuito y la tarea pendiente antes de la intervención. Al reaparecer, un aviso indica por qué ocurrió la descarga para que el jugador pueda corregir la causa y retomar el trabajo.

## Progresión del electricista

El **cuaderno de campo** guarda los aprendizajes, permite repetir prácticas con herramientas eléctricas y desbloquea ayudas de planificación al completar retos. Las primeras pistas se leen directamente en el mapa y en el informe de cada prueba.

| Habilidad | Reto práctico | Herramienta que se desbloquea |
| --- | --- | --- |
| Evaluar un sitio | Comparar dos ubicaciones para un panel a distintas horas. | Vista de horas de sol directo. |
| Gestionar el consumo | Mover el bombeo de agua a las horas de mayor generación y comprobar que el tanque sigue listo a tiempo. | Calendario para programar cargas flexibles. |
| Diagnosticar interrupciones | Detectar cuándo se agota una batería durante la jornada. | Gráfica de carga para comparar diseños entre pruebas. |
| Revisar el cableado | Localizar y corregir un cortocircuito en una instalación de práctica. | Lista de comprobación de rutas, aislamiento y conexiones. |

### Árbol de habilidades

El cuaderno de campo muestra el árbol y las condiciones de cada nodo. Las **competencias técnicas** se obtienen al completar prácticas. Abren familias de cables, herramientas de diagnóstico y encargos que requieren aplicarlas. Las **mejoras de oficio** se compran con créditos de campaña después de aprender la competencia asociada. Cada nodo muestra su costo y el efecto que tendrá en la instalación.

| Rama | Competencia previa | Ejemplo de mejora de oficio |
| --- | --- | --- |
| Cableado | Completar la práctica de canalización interior. | **Aprovechamiento de cable.** Reduce el cable presupuestado para recortes y sobrantes de un tramo. El trazado físico conserva su longitud y el tramo sigue sujeto a sus comprobaciones. |
| Diagnóstico | Localizar un fallo con el multímetro. | **Inspección rápida.** El informe acota el fallo a menos puntos del circuito para agilizar la revisión. |
| Planificación energética | Programar una carga flexible en una práctica. | **Comparación de diseños.** Permite ver juntas dos pruebas para comparar el costo y la reserva de energía. |

### Volver a encargos anteriores

El jugador puede reabrir un encargo completado y partir de una copia de su mejor instalación o de un diseño nuevo. Puede usar las competencias, las familias de cables y las mejoras que ya tenga desbloqueadas, siempre que las piezas sean compatibles con el mapa y el encargo. La revisión se evalúa con los mismos requisitos, presupuesto y objetivo de excelencia. El juego conserva la mejor calificación conseguida.

#### Potenciadores por mejorar instalaciones

Los encargos con recompensa muestran en el mapa de campaña su **potenciador único** y la calificación necesaria para desbloquearlo. Así, un establecimiento pequeño puede seguir siendo valioso aunque su aporte base sea bajo. El potenciador se obtiene una sola vez al alcanzar la calificación indicada, tanto en la primera entrega como al mejorar el diseño en una visita posterior. Queda activo en los siguientes cierres de campaña.

Por ejemplo, *La primera aula* ofrece **+5 % de créditos al cierre del día** al alcanzar tres estrellas. Si el jugador vuelve con **Aprovechamiento de cable** y alcanza la calificación, el bono se aplica a los ingresos de todos los establecimientos en servicio. Con un aporte base conjunto de 200 créditos, cobraría 210 en el siguiente cierre. Si hay varios potenciadores porcentuales, sus porcentajes se suman y se aplican una vez al total base.

### Prácticas con herramientas eléctricas

En la primera práctica de cada herramienta, el jugador aprende para qué sirve, la elige y la usa en una tarea guiada. El juego señala qué ocurrió y permite repetir el ejercicio desde el cuaderno de campo. Las prácticas de montaje y continuidad se realizan con el circuito simulado desenergizado.

| Herramienta | Qué aprende el jugador | Práctica en primera persona |
| --- | --- | --- |
| Pelacables | Retirar el aislamiento de un extremo sin dañar el conductor. | Preparar un cable para conectarlo al tablero. |
| Destornillador aislado | Elegir la punta adecuada y asegurar un borne. | Fijar un conductor y revisar si quedó suelto. |
| Multímetro | Interpretar una lectura de continuidad. | Comprobar si un tramo del circuito ofrece un camino continuo y localizar una conexión abierta. |
