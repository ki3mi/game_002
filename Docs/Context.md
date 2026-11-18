# Luz de vereda

## Concepto del juego

*Luz de vereda* es un juego de puzles de construcción y simulación en 3D low poly. El jugador encarna a un electricista que lleva energía a colegios rurales. En cada nivel recorre el colegio en tercera persona, conoce las necesidades de la comunidad y construye una instalación con piezas modulares. Al pulsar **Probar**, puede ver si su diseño mantiene en funcionamiento las actividades previstas durante una jornada simulada.

Las condiciones del mapa cambian el problema. Un árbol proyecta sombra sobre el patio, una ladera encarece el transporte entre sedes o varios días nublados reducen la energía disponible. El jugador puede resolver un mismo encargo con distintas combinaciones de ubicación, generación, almacenamiento y consumo.

## ODS 7 y fuentes de energía

Todas las instalaciones del juego se alimentan de fuentes renovables. La primera etapa usa energía solar fotovoltaica. En mapas posteriores se propone incorporar pequeñas turbinas eólicas donde haya viento aprovechable y microhidráulica donde el caudal del mapa permita usarla. Las baterías almacenan el excedente para usarlo más tarde y las luminarias eficientes reducen la demanda.

El diseño traduce tres metas del ODS 7 a decisiones jugables. [Naciones Unidas en Colombia — ODS 7](https://colombia.un.org/es/sdgs/7)

| Meta | Cómo se juega |
| --- | --- |
| **7.1 · Acceso asequible y fiable** | Mantener los servicios del colegio en sus horarios y gestionar el costo de la instalación. |
| **7.2 · Energía renovable** | Elegir la fuente y su ubicación según los recursos del mapa. |
| **7.3 · Eficiencia energética** | Reducir el consumo para mantener el mismo servicio con menos energía. |

## Ciclo de juego y economía

1. **Inspeccionar.** El encargo muestra los servicios obligatorios, sus horarios, el presupuesto y el objetivo de excelencia. El jugador recorre el lugar y puede adelantar la hora para observar cómo se mueve la sombra.
2. **Construir.** Coloca y conecta componentes en el mapa. La interfaz actualiza el costo en créditos según los equipos, las conexiones y, cuando el nivel lo incluye, el transporte. El presupuesto es independiente para cada colegio.
3. **Probar.** La simulación avanza por las franjas del escenario. La generación cubre primero el consumo del momento. El excedente carga la batería y la energía almacenada cubre los déficits mientras quede reserva. Si se interrumpe un servicio obligatorio, el informe indica la hora y el motivo.
4. **Ajustar o entregar.** El jugador puede mover, sustituir y retirar piezas, y repetir la prueba cuantas veces quiera. Cada intento se evalúa por el costo de su diseño final. Una estrella basta para avanzar y se puede volver a mejorar la solución.

### Calificación

| Resultado | Condición |
| --- | --- |
| **Nivel incompleto** | Incumple al menos un requisito obligatorio del encargo. |
| **★** | Cumple todos los requisitos obligatorios y supera el presupuesto. |
| **★★** | Cumple todos los requisitos obligatorios dentro del presupuesto, con el objetivo de excelencia pendiente. |
| **★★★** | Cumple todos los requisitos obligatorios y el objetivo de excelencia dentro del presupuesto. Representa una instalación perfecta para ese encargo. |

El presupuesto admite un costo igual a su límite. Un colegio puede pedir como objetivo de excelencia terminar con una reserva mínima de batería o mantener el servicio durante una prueba adicional de clima adverso.

## Progresión del electricista

El **cuaderno de campo** guarda los aprendizajes y desbloquea herramientas reutilizables al completar retos prácticos. Las primeras pistas se leen directamente en el mapa y en el informe de cada prueba.

| Habilidad | Reto práctico | Herramienta que se desbloquea |
| --- | --- | --- |
| Evaluar un sitio | Comparar dos ubicaciones para un panel a distintas horas. | Vista de horas de sol directo. |
| Gestionar el consumo | Mover el bombeo de agua a las horas de mayor generación y comprobar que el tanque sigue listo a tiempo. | Calendario para programar cargas flexibles. |
| Diagnosticar interrupciones | Detectar cuándo se agota una batería durante la jornada. | Gráfica de carga para comparar diseños entre pruebas. |

## Primer nivel jugable · La primera aula

Una clase comunitaria necesita luz continua de 18:00 a 20:00. El presupuesto es de **100 créditos** y el objetivo de excelencia es terminar con al menos **20 % de batería**. El jugador cuenta con dos sitios para el panel: un patio con sombra parcial y una azotea soleada que exige una conexión más larga.

Para el prototipo, el panel y la batería cuestan 35 créditos cada uno. La batería tiene capacidad de 6 unidades de energía y empieza vacía. Las conexiones al patio y a la azotea cuestan 5 y 20 créditos, respectivamente. En el patio, el panel genera 4 unidades antes de la clase y en la azotea genera 6. Durante la clase, el aula depende de la energía almacenada. La luminaria estándar cuesta 15 créditos y consume 4 unidades en esas dos horas. La eficiente cuesta 25 y consume 2 unidades. Estas cifras son valores de juego.

| Diseño probado | Costo | Resultado |
| --- | ---: | --- |
| Panel en el patio y luminaria estándar, sin batería | 55 | **Nivel incompleto.** El aula queda a oscuras desde el inicio de la clase. |
| Panel en la azotea, batería y luminaria estándar | 105 | **★** · El aula tiene luz durante toda la clase, con sobrecosto. |
| Panel en el patio, batería y luminaria estándar | 90 | **★★** · La clase tiene luz hasta el final y la batería queda vacía. |
| Panel en el patio, batería y luminaria eficiente | 100 | **★★★** · La clase tiene luz hasta el final y queda un 33 % de batería. |

Al terminar la prueba, el jugador ve el aula desde el patio y puede recorrer la simulación hasta el momento en que se apagaron las luces o terminó la clase.
