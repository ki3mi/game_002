# Luz de vereda

## Concepto del juego

*Luz de vereda* es un juego de puzles de construcción y simulación en 3D low poly. El jugador encarna a un electricista que lleva energía a colegios rurales. En cada nivel recorre el colegio, conoce las necesidades de la comunidad y construye una instalación con piezas modulares. Trabaja en primera persona al realizar conexiones, configurar equipos y hacer verificaciones. Al pulsar **Probar**, puede ver si su diseño mantiene en funcionamiento las actividades previstas durante una jornada simulada.

Las condiciones del mapa cambian el problema. Un árbol proyecta sombra sobre el patio, una ladera encarece el transporte entre sedes o varios días nublados reducen la energía disponible. El jugador puede resolver un mismo encargo con distintas combinaciones de ubicación, generación, almacenamiento y consumo.

Consulta las reglas y los sistemas de juego en [Mecánicas de Luz de vereda](Mecanicas.md).

## ODS 7 y fuentes de energía

Todas las instalaciones del juego se alimentan de fuentes renovables. La primera etapa usa energía solar fotovoltaica. En mapas posteriores se propone incorporar pequeñas turbinas eólicas donde haya viento aprovechable y microhidráulica donde el caudal del mapa permita usarla. Las baterías almacenan el excedente para usarlo más tarde y las luminarias eficientes reducen la demanda.

El diseño traduce tres metas del ODS 7 a decisiones jugables. [Naciones Unidas en Colombia — ODS 7](https://colombia.un.org/es/sdgs/7)

| Meta | Cómo se juega |
| --- | --- |
| **7.1 · Acceso asequible y fiable** | Mantener los servicios del colegio en sus horarios y gestionar el costo de la instalación. |
| **7.2 · Energía renovable** | Elegir la fuente y su ubicación según los recursos del mapa. |
| **7.3 · Eficiencia energética** | Reducir el consumo para mantener el mismo servicio con menos energía. |

## ODS 4 y aprendizaje práctico

El juego contribuye al ODS 4, **Educación de calidad**, al introducir competencias técnicas básicas mediante retos de instalación y diagnóstico. Se vincula con la meta **4.4**, que promueve el desarrollo de competencias técnicas y profesionales. [Naciones Unidas en Colombia — ODS 4](https://colombia.un.org/es/sdgs/4)

## Primer nivel jugable · La primera aula

Una clase comunitaria necesita luz continua de 18:00 a 20:00. El presupuesto es de **100 créditos** y el objetivo de excelencia es terminar con al menos **20 % de batería**. El jugador cuenta con dos sitios para el panel: un patio con sombra parcial y una azotea soleada que exige una conexión más larga.

El jugador compara ambos sitios con la cámara de su elección. Al interactuar con el tablero, pasa automáticamente a primera persona para fijar un conductor con el destornillador y verificar la continuidad de la conexión con el multímetro. Al salir del tablero, recupera su cámara anterior y puede pulsar **Probar** para simular la jornada.

Para el prototipo, el panel y la batería cuestan 35 créditos cada uno. La batería tiene capacidad de 6 unidades de energía y empieza vacía. Las conexiones al patio y a la azotea cuestan 5 y 20 créditos, respectivamente. En el patio, el panel genera 4 unidades antes de la clase y en la azotea genera 6. Durante la clase, el aula depende de la energía almacenada. La luminaria estándar cuesta 15 créditos y consume 4 unidades en esas dos horas. La eficiente cuesta 25 y consume 2 unidades. Estas cifras son valores de juego.

| Diseño probado | Costo | Resultado |
| --- | ---: | --- |
| Panel en el patio y luminaria estándar, sin batería | 55 | **Nivel incompleto.** El aula queda a oscuras desde el inicio de la clase. |
| Panel en la azotea, batería y luminaria estándar | 105 | **★** · El aula tiene luz durante toda la clase, con sobrecosto. |
| Panel en el patio, batería y luminaria estándar | 90 | **★★** · La clase tiene luz hasta el final y la batería queda vacía. |
| Panel en el patio, batería y luminaria eficiente | 100 | **★★★** · La clase tiene luz hasta el final y queda un 33 % de batería. |

Al terminar la prueba, el jugador ve el aula desde el patio y puede recorrer la simulación hasta el momento en que se apagaron las luces o terminó la clase.

## Nivel avanzado · La escuela en la ladera

La zona con mejor exposición solar está en una terraza alejada del aula. El jugador construye un recinto para el tablero y las baterías, elige por dónde llevar la línea aérea y continúa el recorrido por el interior de una pared al llegar al colegio. Una ruta más directa usa menos cable, mientras que bordear el terreno puede facilitar la ubicación de los postes.
