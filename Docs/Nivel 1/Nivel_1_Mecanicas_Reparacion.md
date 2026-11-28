# Nivel 1 · Mecánicas de reparación eléctrica

El jugador diagnostica y repara averías con el colegio desenergizado. El suministro se corta desde la sala principal de mantenimiento en el **Nivel 0** y permanece cortado durante el **Nivel 1**. Una tarea se completa cuando el jugador obtiene la comprobación que pide el encargo. La instalación sigue apagada después de verificarla. [Mapa del nivel 1](Nivel_1_Etapa_1.md) · [Mecánicas generales](../Mecanicas.md)

## Tutorial en mantenimiento · Nivel 0

1. **Preparar el trabajo.** La cinta de mantenimiento presenta el corte de suministro y explica cómo consultar el estado del tablero simulado antes de intervenir.
2. **Inspeccionar una pieza.** El jugador abre el visor 3D, gira una pieza de práctica y localiza su referencia, su conector o los puntos relevantes del ejercicio.
3. **Aprender continuidad.** En un tablero de práctica desenergizado, compara un tramo continuo con uno abierto y aprende a reconocer el pitido y la indicación de circuito abierto.
4. **Comparar baterías.** En un segundo ejercicio, el jugador usa **V⎓** para comparar dos celdas del mismo tipo con la linterna apagada y consulta el manual lateral para interpretar los voltajes como porcentajes aproximados. En el Nivel 1 puede omitir la medición y elegir directamente qué batería retirar. El miedo sube mientras dura la inspección de cambio. [Linterna y baterías](../Mecanicas.md#linterna-y-baterías)

Si el jugador elige un modo que no corresponde a la comprobación, el tutorial indica que la lectura no sirve para esa tarea y permite repetirla. Si el escenario de práctica todavía indica que el circuito del colegio está energizado, el juego detiene la medición de continuidad y explica qué condición falta. Las lecturas y los puntos interactivos pertenecen a la simulación.

## Kit inicial

| Herramienta | Uso en el juego | Cuándo se introduce |
| --- | --- | --- |
| **Multímetro · Continuidad** | Comprobar si un tramo o componente simulado ofrece un camino continuo. | Tutorial y primera avería en la caja de fusibles. |
| **Multímetro · Voltaje de corriente continua (V⎓)** | Comparar las dos baterías de la linterna antes de elegir cuál reemplazar. | Segundo ejercicio y comprobación opcional en el Nivel 1. |
| **Destornillador** | Abrir tapas y fijar conexiones en el montaje simulado. | Primera reparación. |
| **Pelacables** | Preparar el extremo de un cable de repuesto en un punto interactivo. | Una tarea posterior de reemplazo de cable, después de aprender a identificar la pieza. |

El **Fluke 117** es una referencia visual propuesta. El multímetro del juego ofrece como máximo **dos modos interactivos en el Nivel 1**. Cada reparación pide solo el modo que ayuda a resolverla. [Ficha oficial del Fluke 117](https://www.fluke.com/en-us/product/electrical-testing/digital-multimeters/fluke-117)

## Primera reparación · Caja de fusibles de la sala de limpieza

El jugador retira los tornillos, diagnostica tres fusibles y busca el repuesto que falta antes de cerrar la caja. [Plan de implementación](Nivel_1_Plan_Caja_Fusibles.md)

## Reparación posterior · Tramo interrumpido del corredor

1. Abrir la caja de paso con el destornillador. Leer las etiquetas y medir la continuidad del tramo señalado para localizar la interrupción.
2. Seguir una pista hasta el aula contigua y recuperar el repuesto escondido. Girarlo en el inventario 3D para comprobar su referencia y la forma del conector.
3. Volver a la caja, instalar la pieza y asegurar la conexión. Si el jugador interrumpe el trabajo para mirar alrededor con la linterna, conserva las conexiones ya confirmadas.
4. Repetir la comprobación de continuidad. La lista de tareas registra la reparación al obtener la lectura requerida. El corredor permanece oscuro.

## Reparaciones candidatas posteriores

| Ubicación | Tarea candidata | Comprobación |
| --- | --- | --- |
| **Aula** | Encontrar el conector faltante de una luminaria e identificarlo por su referencia en la vista 3D. | Continuidad del tramo indicado en el encargo. |
| **Baños** | Distinguir dos cables del mismo color por sus etiquetas y corregir los recorridos con ayuda de un esquema. | Continuidad entre los puntos previstos de cada recorrido. |
| **Sala de docentes** | Identificar un fusible abierto y comparar su referencia con la del repuesto. | Continuidad del repuesto en el ejercicio. |
| **Biblioteca** | Localizar un tramo abierto entre varios ramales que tienen etiquetas parecidas. | Continuidad entre los extremos del tramo indicado por la ficha de mantenimiento. |
