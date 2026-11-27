# Nivel 0 · Introducción de Just a Wiring Job?

El jugador empieza al volante a las **16:00** y llega al colegio a las **18:00**. La carretera presenta el aislamiento del lugar. El tramo a pie enseña hacia dónde ir y qué sucede al alejarse del colegio. La introducción termina cuando el jugador ha visto la cinta de mantenimiento y cruza hacia la sección del nivel 1. [Contexto del juego](../Context.md) · [Arquitectura](../Arquitectura.md)

## Recorrido general

| Tramo | Acción del jugador | Qué marca el avance |
| --- | --- | --- |
| **Autopista en el bosque** | Tras un plano que sigue a la camioneta en movimiento, recibe el control desde el asiento del conductor y sigue la carretera. | La luz diurna baja a medida que avanza hacia el colegio. |
| **Llegada** | Toma el desvío y entra despacio al estacionamiento. La cámara muestra el colegio, la caseta y la sala de mantenimiento. | La hora llega a las 18:00. Al terminar la cinemática, recibe el control a pie junto al vehículo. [Plan de las cinemáticas](Nivel_0_Etapa_4.md) |
| **Caseta de vigilancia** | Se acerca a la puerta principal y oye la voz del guardia desde la caseta. | El guardia indica que el jefe dejó una videocasetera con instrucciones en mantenimiento. Se habilita el paso peatonal. |
| **Recorrido de servicio** | Sigue un sendero iluminado y la señalización de mantenimiento junto a la fachada. También puede desviarse a pie hacia el bosque. | Encuentra la sala de mantenimiento. Si exploró el bosque, puede recuperar la calma al volver a la entrada. |
| **Sala de mantenimiento** | Identifica el equipo de video, recoge la cinta y la reproduce. | El encargo de la primera sección se registra al finalizar la cinta y se habilita su puerta. |

## Autopista y conducción

Montar una carretera asfaltada de dos carriles de unos **3–4 km** como punto de partida. Apuntar a **4–6 minutos de conducción** a ritmo habitual. El perfil combina una recta inicial para aprender los controles, curvas amplias bajo el dosel, una ligera subida y una última curva que revela el colegio. La entrada de servicio es el único desvío anunciado. Una señal reflectante y una luz de la caseta sirven de referencia en el último tramo.

La cámara va a la altura de los ojos del conductor. El tablero y los bordes del parabrisas sirven de referencia al girar. La cámara acompaña con un movimiento leve la aceleración y la frenada. La dirección se suaviza y pierde sensibilidad al subir la velocidad. Como asignación inicial, **W** acelera, **S** frena y permite retroceder desde reposo, y **A/D** giran. El automóvil puede detenerse y corregir la maniobra. En un roce con la barrera, pierde velocidad y conserva el control.

Colocar barreras laterales continuas con colisión junto a ambos arcenes y acompañarlas con postes reflectantes. Validar las juntas, las curvas y las transiciones del pavimento para que el vehículo se mantenga en la vía. Las barreras se abren únicamente en el acceso al estacionamiento. Esculpir la pendiente del ramal y ampliar el espacio junto al portón para que sea posible aparcar sin una maniobra exacta. La salida del auto transfiere la vista al controlador a pie en un punto despejado, del lado del sendero.

## Atardecer y luz del auto

El atardecer avanza con el **mayor progreso alcanzado sobre la carretera**. Retroceder conserva la hora alcanzada y detenerse no adelanta este tramo narrativo. Distribuir las horas de referencia a lo largo del trayecto e interpolar entre ellas para que el cambio de luz sea gradual.

| Momento del viaje | Imagen y sonido |
| --- | --- |
| **16:00 · Salida** | Luz ámbar entre los troncos, sombras largas y detalles visibles a ambos lados de la vía. Se oyen el motor, los neumáticos y el bosque. |
| **16:45–17:20 · Primeras curvas** | El sol pierde altura y el dosel corta la luz directa. La calzada conserva contraste y los fondos se enfrían. Los faros se encienden automáticamente al entrar en esta fase. |
| **17:20–17:50 · Bosque cerrado** | Disminuye la luz ambiental y aparecen tonos azul grisáceo. Los faros pasan a definir las marcas viales, los reflectantes y los troncos del borde. En curvas, la vegetación sale gradualmente del haz. |
| **17:50–18:00 · Acceso al colegio** | El bosque abre un claro. La fachada se lee como una silueta grande y la caseta tiene la luz más reconocible. Al llegar al estacionamiento, se fija la hora en las **18:00** para la entrega al nivel 1. |

Usar dos focos unidos a los faros delanteros y orientados hacia abajo. Sus haces se solapan en la calzada, dejan leer el siguiente giro y producen reflejos localizados en marcas y señales. La iluminación se mueve con el auto y respeta la oclusión de barreras, árboles y taludes. Probar primero un alcance de referencia de **45–60 m** y ajustarlo según la velocidad y las curvas. Mantener una exposición que permita distinguir la calzada desde el parabrisas al caer la noche. Usar el grano y el viñeteado suaves como referencia de imagen. [Integración de la cámara de pasillos](../Integracion-Camara-Pasillos.md)

## Bosque y fachada del colegio

Evaluar la muestra **Fantasy Forest Environment – Free Demo** en `Assets/EcoElectry/ThirdParty/FantasyForest/`. Se conservan `Scenes/demoScene_free.unity`, `Terrain/FantasyForestDemoTerrain.asset` y los prefabs de árbol y pasto. El paquete gratuito ofrece una sola variante de árbol. Completar el paisaje con los árboles y arbustos disponibles en `ResourcePack` si el tramo se ve demasiado repetitivo. Hacer una prueba de materiales, sombras y vegetación con el URP del proyecto antes de extender el terreno. [Ficha oficial del paquete gratuito](https://assetstore.unity.com/packages/3d/environments/fantasy/fantasy-forest-environment-free-demo-35361)

- **Extensión del terreno.** Trabajar con una copia de los datos del terreno y continuar el relieve en módulos contiguos. Emparejar alturas y materiales en las uniones. Modelar una franja estable bajo el asfalto y usar cunetas y taludes para que la autopista parezca integrada en el relieve.
- **Profundidad y rendimiento.** Usar vegetación detallada cerca de las barreras y del camino a pie. Aumentar la densidad visual hacia el fondo con grupos de árboles, relieve y niebla suave. Aprovechar las transiciones de detalle de los árboles y limitar las sombras dinámicas de la vegetación lejana. Medir el recorrido completo en el equipo de prueba antes de decidir si se necesita carga por tramos.
- **Fachada.** Buscar un colegio modular de estilo japonés o construir el volumen principal con piezas compatibles de dos o tres plantas. Priorizar una silueta rectangular, hileras de ventanas, un acceso cubierto y un ala lateral de servicio. Validar escala, materiales URP y lectura desde el parabrisas antes de elegir el recurso. La escena exterior contiene la envolvente del colegio, el estacionamiento, los accesos y la sala de mantenimiento. Las demás dependencias usan las escenas de sus secciones.

## Llegada a pie y guardia

La luz de la caseta atrae al jugador desde el estacionamiento. La voz del guardia se activa al acercarse a la entrada y se posiciona en el interior de la caseta. Puede sonar ligeramente amortiguada por el vidrio o por un intercomunicador. El guardia se representa mediante su voz, subtítulos y la luz de la caseta. El texto y la actuación se definirán después. La información que debe transmitir es que el jefe dejó una videocasetera con una cinta en mantenimiento y cómo llegar allí.

Desde el acceso peatonal, el jugador sigue el borde de la fachada por un recorrido de servicio de **30–60 segundos**. Proponer tres referencias consecutivas, con la caseta visible al inicio, una luminaria de pared en el giro y un rótulo de mantenimiento sobre la puerta final. Dejar visible el camino de regreso desde el último tramo. El guardia puede repetir la indicación si el jugador vuelve a la caseta.

### Desvío hacia el bosque

Dejar una abertura reconocible en la arboleda al borde del estacionamiento, separada del sendero de servicio. A pie, se puede cruzar el claro y entrar entre los árboles. La transición usa zonas continuas para que el jugador pueda avanzar y retroceder por el mismo camino.

| Zona | Comportamiento del miedo | Señales para orientarse |
| --- | --- | --- |
| **Entrada y camino de servicio** | Al regresar a la entrada, el miedo baja gradualmente. La barra desaparece cuando llega a cero. | Caseta, luminarias, fachada y rótulos del colegio. |
| **Borde del bosque** | Al cruzar el límite a pie aparece la barra y empieza a subir lentamente. | La luz de la caseta sigue visible y los sonidos del colegio se atenúan. |
| **Bosque profundo** | La acumulación se acelera conforme el jugador se interna. Al alcanzar el máximo, se desmaya y pierde el intento. | Pulso más fuerte, visión menos clara y temblor creciente. El jugador recibe estas señales con margen para retroceder. |

La velocidad de aumento depende de la profundidad y del tiempo en la zona. Volver desde lo profundo reduce la velocidad de acumulación al llegar al borde. La recuperación empieza al pisar la zona iluminada de la entrada. Mantener el mismo valor al pasar de una franja a otra, para que cruzar repetidamente su límite no reinicie la barra. Para el prototipo, guardar un punto de control después de estacionar. Si el jugador se desmaya, mostrar la derrota y permitir reintentar desde ese punto. Calibrar los umbrales con un recorrido de ida y vuelta a velocidad de marcha. [Reglas de miedo y pánico](../Mecanicas.md#miedo-pánico-y-fin-del-turno)

## Mantenimiento y cinta de instrucciones

La sala es pequeña y funcional. Preparar un banco de trabajo con tornillo de banco, panel de herramientas, armario metálico, estantes con cajas, carretes de cable y un tablero de repuestos. Usar una luz de trabajo cálida junto al punto interactivo y un fluorescente frío que revele el resto de la sala. Colocar una **videocasetera VHS, un televisor CRT y una cinta identificada como el encargo** sobre el banco. Desde la puerta se debe reconocer el equipo de video antes de acercarse.

1. **Encontrar la cinta.** El jugador la recoge del banco y la reconoce por su etiqueta. La acción de inspección puede reutilizar el visor 3D del inventario cuando esté disponible.
2. **Reproducirla.** Interactúa con la videocasetera. El televisor muestra la grabación y los subtítulos presentan sus instrucciones. El jugador puede repetirla si quiere repasar una indicación.
3. **Recibir el encargo.** La cinta explica qué sección atender, qué quedó a medio hacer, qué piezas o servicios deben comprobarse y cómo se verifica que el trabajo terminó. El contenido exacto y los diálogos se escriben tras definir las tareas del nivel 1. Al terminar la reproducción, el jugador conserva un resumen consultable.
4. **Entrar al nivel 1.** La puerta señalizada de la primera sección se habilita tras registrar la grabación. Al cruzarla, carga la escena interior y empieza el primer turno a las **18:00**. El reloj de reparación empieza en esta transición, de modo que explorar el exterior y escuchar las instrucciones tienen su propio ritmo.

## Orden de implementación y verificación

| Entrega | Prueba de aceptación |
| --- | --- |
| **1. Ruta y vehículo** | La escena de introducción abre por separado. Se puede conducir hasta el colegio, frenar y corregir el estacionamiento. Las barreras mantienen el auto en la ruta incluso al rozarlas. |
| **2. Terreno, fachada y luces** | El bosque continúa a ambos lados de la vía y las uniones de terreno se mantienen fuera de vista. Los faros permiten anticipar curvas y quedan ocluidos por la geometría. La fachada se descubre desde el último tramo y el reloj marca las 18:00 al llegar. |
| **3. Llegada y desvío** | Se puede bajar del auto, localizar la voz en la caseta y caminar hasta mantenimiento. El miedo aparece al entrar al bosque, baja y se oculta al regresar a la entrada, y llega al desmayo si el jugador continúa hacia lo profundo. |
| **4. Cinta y transición** | La cinta se puede localizar, reproducir y revisar. Sus instrucciones quedan consultables. La puerta conduce a la escena designada del nivel 1 tras reproducirla y el tiempo del turno se inicializa una sola vez. |

Probar el recorrido completo de día a noche y una segunda pasada con parada en carretera, vuelta desde el bosque y reproducción repetida de la cinta. Usar una escena propia para el nivel 0 y una escena de sección independiente para el nivel 1. La prueba de pasillos sirve como referencia para la cámara y la linterna del tramo a pie. [Prueba de los pasillos](../Prueba-Prototipo-Pasillos.md)
