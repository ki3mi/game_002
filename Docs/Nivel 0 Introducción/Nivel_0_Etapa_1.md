# Nivel 0 · Etapa 1 · Construcción del mapa

Montar una escena exterior independiente con una autopista que atraviesa el bosque y termina en el acceso a un colegio rodeado de vegetación. La entrega abarca terreno, carretera, edificios, recorridos peatonales, utilería estática y colisiones de la geometría. La iluminación, el atardecer, la conducción y las interacciones se implementarán en etapas posteriores. [Diseño de la introducción](Nivel_0_Introducción.md) · [Arquitectura](../Arquitectura.md)

## Escena y recursos

Crear `Assets/EcoElectry/Content/Scenes/Level00-Introduction.unity`. Guardar los terrenos propios del mapa en `Content/Terrain/Level00/` y sus prefabs adaptados en `Content/Prefabs/Level00/`, bajo `Assets/EcoElectry/`. Organizar la escena en `Terrain`, `Highway`, `Forest`, `SchoolExterior`, `Maintenance` y `Markers`. La revisión de esta etapa se realiza abriendo la escena directamente en el Editor.

| Recurso | Uso en esta etapa |
| --- | --- |
| **Fantasy Forest Environment Free Sample** | Partir de la escena de muestra, su terreno y sus prefabs de árbol y pasto. Copiar los datos de terreno antes de esculpirlos y prolongar el paisaje con módulos de terreno propios. |
| **Ezereal Assets · Electric Truck** | Usar `ThirdParty/Ezereal/ElectricTruck/Prefabs/Electric Truck.prefab` como referencia del ancho, longitud y altura del vehículo. Colocarlo a escala original y dejar su `Rigidbody` cinemático en la instancia de prueba. Usar esa referencia al revisar las curvas, el estacionamiento y el espacio para bajar. |
| **Kansai University (Takatsuki) · Building A** | Usar `ThirdParty/Kansai/BuildingA/Prefabs/A_Tou.prefab` como fachada principal. Ajustar la escala de una instancia o variante del nivel. |
| **ResourcePack** | Completar la vegetación con otras siluetas de árbol y arbusto, y revisar los prefabs de portón, entrada peatonal y cercas para el perímetro. Usar volúmenes provisionales para las piezas que aún no tengan un modelo apropiado. |

## Orden de montaje

### 1. Trazar el recorrido y esculpir el terreno

1. Dibujar primero un eje que conecte **recta inicial → curvas bajo el bosque → subida suave → curva de revelación → ramal → estacionamiento**. Los **3–4 km** de la introducción son una referencia para el trazado completo. Revisar un tramo representativo con el vehículo estático antes de extender la carretera a esa longitud.
2. Crear módulos de terreno alrededor del eje y emparejar sus bordes en altura y textura. Formar una franja continua bajo la calzada, desniveles suaves en los márgenes y una explanada cerca del colegio. Colocar las uniones fuera de la trayectoria principal o alisarlas de modo que queden sin grietas ni escalones.
3. Mantener los árboles y el relieve entre la carretera y el colegio durante la mayor parte del recorrido. Abrir el claro después de la última curva para que se revele la fachada. Desde el estacionamiento se debe seguir viendo bosque en los laterales y detrás del edificio.

### 2. Construir la autopista y su ramal

- Construir dos carriles asfaltados con arcenes, demarcación y barreras laterales físicas. Comenzar con **3–3,5 m por carril** como ancho de bloque de prueba. Validar la relación entre carril, barrera y camioneta en planta y desde la altura del asiento.
- Usar malla continua o módulos compatibles de recta, curva y transición. Ajustar pendientes y uniones para que los tramos formen una superficie de colisión continua. Cerrar los huecos entre barreras en curvas y uniones.
- Abrir las barreras en el ramal señalizado al colegio. Diseñar una transición más amplia que el ancho de la camioneta y un estacionamiento con espacio para corregir la posición. Dejar espacio junto a la cabina para bajar del vehículo y un paso despejado desde el estacionamiento hasta la caseta.

### 3. Colocar y escalar Building A

1. Instanciar `A_Tou` vinculado al prefab y medir su volumen visible en Unity en la escala importada `(1, 1, 1)`. Comparar su fachada con una referencia humana, la camioneta y el portón. Ajustar el claro y el retranqueo antes de corregir el tamaño del edificio.
2. Si la proporción sigue fuera de escala, aplicar un **factor uniforme** a la instancia y registrar su valor y las dimensiones resultantes. Iterar desde dos puntos, la última curva y el sendero peatonal. Mantener una lectura clara de las plantas y las ventanas sin deformar la fachada. Ajustar la explanada al edificio para que la base quede asentada.
3. Revisar las cajas de colisión y el `MeshCollider` que ya trae el prefab. Comprobar que el perímetro bloquea el paso donde corresponde y que ninguna caja invade el sendero. Si una colisión impide un acceso necesario, ajustar únicamente la variante o instancia del nivel. Guardar el prefab importado como referencia de origen.

### 4. Completar el bosque y el acceso a mantenimiento

- **Vegetación.** Distribuir árboles a ambos lados de la autopista y cerrar el fondo del colegio con grupos de distintas alturas. Usar pasto y sotobosque cerca de los puntos que se ven a pie. Mantener libres los arcenes, las señales, el estacionamiento y el paso al portón. Desde las vistas principales, ocultar los bordes del terreno con relieve y masa forestal.
- **Entrada.** Colocar estacionamiento, portón, acceso peatonal, caseta y cerco. El recorrido a pie conduce primero a la caseta. Desde allí, un sendero junto a la fachada lleva a una puerta de mantenimiento reconocible por su rótulo. Dejar un acceso distinto hacia el borde del bosque que permita ir y volver a la entrada por el mismo camino.
- **Sala.** Montar suelo, paredes, techo y puerta alrededor de un espacio con banco de trabajo, estantería, armario y herramientas. Reservar sobre el banco la posición de la videocasetera, la cinta y el CRT mediante modelos estáticos o volúmenes rotulados. La sala puede adosarse a Building A para que su acceso sea transitable sin depender de un interior del prefab de fachada.
- **Anclajes.** Colocar referencias vacías para el inicio del vehículo, el estacionamiento, la salida peatonal, la voz futura del guardia, el borde y la profundidad del bosque, la videocasetera y la puerta hacia el nivel 1. Estos puntos indican posición y orientación para las etapas siguientes.

## Revisión y criterios de aceptación

| Revisar en el Editor | Resultado esperado |
| --- | --- |
| **Trazado completo** | Desde el punto de inicio se puede seguir visualmente la autopista hasta el ramal. La calzada y las barreras conservan continuidad en pendientes, curvas y juntas. El ramal admite la camioneta a escala original. |
| **Revelación del colegio** | El bosque rodea el claro y oculta el colegio en los primeros tramos. Al salir de la última curva se distinguen Building A, el portón y la caseta. |
| **Escala y colisiones** | La camioneta cabe en el estacionamiento y el paso peatonal tiene anchura suficiente. Se registran el factor de escala de Building A y sus dimensiones finales. Los colliders se revisan con la geometría visible. |
| **Recorrido a pie** | Se distinguen y conectan el estacionamiento, la caseta, el sendero y la sala. La abertura hacia el bosque se lee como un desvío distinto. Desde el banco de trabajo se identifica la salida hacia la primera sección. |
| **Entrega de la escena** | La escena abre por separado, con sus terrenos y prefabs resueltos. Los marcadores se identifican por nombre y se registra qué objetos se montaron con geometría provisional. |
