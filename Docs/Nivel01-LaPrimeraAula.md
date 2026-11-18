# Nivel 01 · La primera aula

[Contexto de EcoElectry](Context.md) · [Mecánicas generales](Mecanicas.md)

## Encargo

El electricista del programa municipal llega a un colegio rural. Una clase comunitaria necesita **luz continua de 18:00 a 20:00**. El encargo tiene un presupuesto de **100 créditos** y el objetivo de excelencia es terminar la clase con **al menos un 20 % de batería**.

## Llegada y conversaciones

El jugador comienza en tercera persona junto a la entrada. Puede alternar la cámara durante la exploración. Dos NPC le presentan el establecimiento y activan tareas en el mapa.

| NPC | Qué muestra | Tareas que añade |
| --- | --- | --- |
| Docente, en la entrada del aula | El salón donde se imparte la clase y el horario que necesita iluminación. | Inspeccionar el aula y comparar dónde colocar el panel. |
| Encargado del colegio, junto al tablero | El tablero con las cuchillas y el espacio previsto para la batería. Señala el patio con sombra parcial y la azotea soleada, que exige un tendido más largo. | Inspeccionar el tablero y dejar el circuito sin energía antes de intervenir. |

Cada intervención aparece en una **burbuja en la parte inferior de la pantalla**, con el nombre del NPC y una indicación para avanzar. Al cerrar el diálogo, las tareas nuevas se añaden a la **lista de tareas en la esquina superior izquierda**. La lista destaca la tarea activa y marca las completadas. El tutorial señala en el escenario el objetivo activo y muestra una instrucción breve al acercarse.

## Tutorial de instalación

1. **Inspeccionar y decidir.** El jugador compara el patio y la azotea, observa cómo cambia la sombra y elige dónde colocar el panel. La vista previa muestra el costo del tendido y actualiza el presupuesto. Al elegir la luminaria, puede comparar el consumo de la estándar y la eficiente.
2. **Dejar el circuito sin energía.** Al interactuar con el tablero, la cámara cambia automáticamente a primera persona. En el montaje guiado inicial, el tutorial pide bajar las cuchillas del establecimiento y espera a que el circuito simulado esté sin tensión y aislado de las fuentes disponibles, incluida la batería si ya está instalada. En las correcciones posteriores, el jugador debe revisar ese estado por su cuenta.
3. **Montar y conectar.** El jugador coloca el panel, la batería si decide usarla y la luminaria. Traza el cable fotovoltaico y la canalización interior. Al intervenir en una conexión, el tutorial señala el punto de trabajo y presenta las herramientas en contexto. El pelacables prepara el extremo del conductor y el destornillador fija el conductor al borne. Las rutas interiores se representan con líneas punteadas.
4. **Verificar con el multímetro.** En primera persona y con el circuito simulado desenergizado, el jugador selecciona la función de continuidad y comprueba un tramo señalado. La respuesta del instrumento indica si hay un camino continuo entre los puntos del ejercicio. Si el tramo está abierto, el tutorial pide revisar la conexión y repetir la medición. Esta comprobación marca el tramo revisado en la lista de tareas.
5. **Probar la jornada.** Al terminar la intervención, el jugador recupera su vista anterior. El tutorial le indica que ponga el tablero simulado en estado de servicio y pulse **Probar**. La simulación carga la batería durante el día si hay una instalada y comprueba si la luz se mantiene encendida durante toda la clase.

## Revisión y cierre

Al finalizar la prueba, el jugador ve el aula desde el patio. El informe muestra el estado de la iluminación de 18:00 a 20:00, el costo de la instalación y la reserva final de batería. Puede revisar el momento de una interrupción en la simulación.

- **Si falta un requisito**, el nivel sigue abierto. El informe indica qué falta y, si hubo un corte, su hora y causa. La lista vuelve a destacar la acción pendiente, como completar una conexión o revisar el almacenamiento. El jugador conserva su diseño, lo corrige y vuelve a probar. Para volver a manipular conexiones, pasa otra vez por el paso de dejar el circuito sin energía.
- **Si la clase tiene luz continua**, el nivel se completa y muestra la calificación. Una instalación por encima de 100 créditos obtiene **★**. Una que cumple dentro del presupuesto obtiene **★★**. Si además termina con al menos un 20 % de batería, obtiene **★★★**. La primera entrega válida cierra el día de campaña según las reglas generales.

### Descarga y punto de guardado

Si el jugador intenta corregir un cable o borne con el circuito aún bajo tensión, por ejemplo porque olvidó bajar las cuchillas después de la prueba, recibe una descarga eléctrica y reaparece en el último punto de guardado automático, cerca del tablero. El mensaje de recuperación le recuerda revisar el estado del circuito antes de tocar la conexión.

El nivel guarda automáticamente al cerrar el informe de una prueba que requiere correcciones, antes de que el jugador vuelva a intervenir. Al reaparecer, se restauran el diseño guardado, el estado del tablero y la tarea pendiente. El jugador puede bajar las cuchillas y retomar la corrección desde ese punto.

## Valores del prototipo

| Pieza o variante | Costo | Comportamiento en el nivel |
| --- | ---: | --- |
| Panel solar | 35 créditos | Genera 4 unidades antes de la clase en el patio o 6 en la azotea. |
| Batería | 35 créditos | Empieza vacía y almacena hasta 6 unidades. |
| Tendido desde el patio | 5 créditos | Conecta el panel del patio con la instalación. |
| Tendido desde la azotea | 20 créditos | Conecta el panel de la azotea con la instalación. |
| Luminaria estándar | 15 créditos | Consume 4 unidades durante la clase. |
| Luminaria eficiente | 25 créditos | Consume 2 unidades durante la clase. |

Durante la clase, el aula depende de la energía almacenada. Las cifras son valores de juego.

| Diseño de prueba | Costo | Resultado esperado |
| --- | ---: | --- |
| Patio y luminaria estándar, sin batería | 55 | **Incompleto.** El aula queda a oscuras desde las 18:00. |
| Azotea, batería y luminaria estándar | 105 | **★** · Iluminación continua con sobrecosto. |
| Patio, batería y luminaria estándar | 90 | **★★** · Iluminación continua, con la batería vacía al terminar. |
| Patio, batería y luminaria eficiente | 100 | **★★★** · Iluminación continua y un 33 % de batería al terminar. |
