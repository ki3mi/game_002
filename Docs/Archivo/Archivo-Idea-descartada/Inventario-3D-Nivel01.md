# Inventario 3D · Nivel 01, La primera aula

Registro compartido entre desarrollo y modelado para el [mapa del nivel 01](Plan-Mapa-Nivel01.md). El colegio, el bosque y su vía de llegada están montados en [Level01-LaPrimeraAula.unity](../Assets/EcoElectry/Content/Scenes/Level01-LaPrimeraAula.unity). El terreno, sus capas y la pintura de la carretera se guardan en [Content/Terrain/Level01](../Assets/EcoElectry/Content/Terrain/Level01).

## Prefabs existentes seleccionados

Cada nombre se resuelve como `Assets/ResourcePack/<familia>/Prefabs/<nombre>.prefab`. Las piezas se integraron como instancias vinculadas a escala de importación. Las poses de puertas y la configuración de componentes se guardan en la escena.

| Familia | Prefabs existentes | Entorno | Uso en el mapa | Estado |
| --- | --- | --- | --- | --- |
| `Arbol` | `ArbolGrande`, `ArbolMediano`, `ArbolPequeno` | Bosque, carretera y patio | Las tres variantes forman el corredor y la transición al claro. Un árbol mediano da sombra parcial al emplazamiento del panel del patio. | En escena |
| `Arbusto` | `ArbustoBajo`, `ArbustoFlorido`, `Seto` | Bosque, entrada y patio | Sotobosque y jardinería baja a los lados de los pasos. | En escena |
| `Montana` | `LomaFondo`, `MontanaFondo` | Horizonte | Lomas y montañas más allá de las zonas transitables. | En escena |
| `CaminoPatio` | `CaminoAfirmado`, `CaminoConcreto`, `CaminoAdoquinado` | Carretera, entrada, patio y ruta técnica | Afirmado en el tramo recto final y el ramal peatonal. Adoquín hacia la entrada y la cancha. Concreto hacia las aulas y la sala. | En escena |
| `AccesoEscolar` | `PuertaPeatonal`, `PortonVehicular` | Entrada principal | La puerta peatonal queda abierta por su bisagra. El portón cerrado marca el final de la vía. | En escena |
| `Cerca` | `CercaMalla`, `CercaRejaMetalica` | Perímetro | Reja sobre el frente. Malla en laterales y fondo. | En escena |
| `SueloPatio` | `SueloConcreto`, `SueloBaldosaExterior` | Patio y cubierta solar | Baldosa exterior en la plaza central y concreto bajo la cubierta. | En escena |
| `EdificioAula` | `AulaModularBasica`, `AulaModularAmpliada` | Aula comunitaria y aula secundaria | Ambas fachadas miran al patio. La primera aprovecha sus escalones y la segunda usa una rampa corta hasta la galería. | En escena |
| `CarpetaEscolar`, `Silla`, `EscritorioDocente` | `CarpetaIndividual`, `CarpetaDoble`, `SillaEscolar`, `SillaDocente`, `EscritorioBasico` | Ambas aulas | Filas con pasillo central en el aula 01 y grupos de pupitres dobles con un pasillo lateral en el aula 02. | En escena |
| `Pizarra`, `LuminariaInterior` | `PizarraTiza`, `PizarraBlanca`, `TuboLED`, `PanelLED` | Ambas aulas | Pizarras en el frente de enseñanza y luminarias como referencias para el montaje posterior. | En escena |
| `SalaElectrica`, `TablerosElectrico` | `SalaElectricaCompacta`, `TableroDistribucion` | Sala eléctrica | El tablero se fija a la pared posterior. Su registro eléctrico está desactivado en la instancia. El suelo queda libre para la futura batería. | En escena |
| `LuminariaExterior`, `Poste` | `LuminariaSolarMural`, `PosteLuz` | Aula comunitaria y patio | Referencias visuales en la fachada y junto al paso hacia la cancha. | En escena |
| `TechoPaneles` | `TechoPanelesPlano` | Cubierta del espacio de servicio | Cubierta de 6,4 × 4,4 m sobre soportes propios. Se dejó un emplazamiento alto al sol y un espacio posterior para el acceso futuro. | En escena |
| `Docente`, `PersonalEscuela` | `DocenteDePie`, `PersonalAdministrativo`, `PersonalMantenimiento` | Aula 01, sala eléctrica y jugador | La docente y el encargado son figuras ambientales. El modelo de mantenimiento representa al jugador y usa el collider de su controlador de movimiento. | En escena |

## Modelos y variantes por preparar

Las piezas esenciales que faltaban en el catálogo se integraron como recursos persistentes de Unity. La tabla registra también las solicitudes resueltas mediante la geometría existente y las piezas opcionales que siguen abiertas.

| Pieza | Entorno | Resolución o requisitos | Estado |
| --- | --- | --- | --- |
| **Canasta de baloncesto** | Cancha | Dos instancias a partir del mismo prefab con soporte, tablero, aro y red. Los soportes tienen colisión y los aros se orientan hacia el centro. [PF_BasketballHoop](../Assets/EcoElectry/Content/Prefabs/Level01/PF_BasketballHoop.prefab) | Entregada |
| **Demarcación de cancha** | Cancha | Losa de 24 × 14 m con colisión continua y mallas de demarcación para perímetro, eje, círculo y zonas de tiro. [PF_BasketballCourt](../Assets/EcoElectry/Content/Prefabs/Level01/PF_BasketballCourt.prefab) | Entregada |
| **Rótulo de la escuela** | Entrada principal | Placa de texto editable sobre pilares con colisión. El patio y las fachadas incorporan señalización de las rutas. [PF_SchoolSign](../Assets/EcoElectry/Content/Prefabs/Level01/PF_SchoolSign.prefab) | Entregado |
| **Accesos transitables de aulas y sala** | Aulas y sala eléctrica | Los colliders de las envolventes ya tienen huecos. Las hojas, manijas y colliders móviles existentes se abrieron a la par mediante ajustes de instancia. | Resuelto con los prefabs existentes |
| **Curvas y transiciones de carretera** | Bosque y llegada | El terreno continuo soporta la vía pintada de 4,6 m y su pendiente. Los módulos de afirmado rematan el tramo recto junto al portón. | Resuelto con terreno y prefabs existentes |
| **Remate de desnivel** | Aula 02 | Rampa corta con malla y collider ajustados a la galería. El aula 01, la sala y la cancha aprovechan sus superficies existentes. [RampaAula02](../Assets/EcoElectry/Content/Prefabs/Level01/Campus/RampaAula02.asset) | Entregado |
| **Rocas bajas y tronco caído** | Márgenes del bosque y exterior del perímetro | Variantes low poly con pivote en el suelo y colisión simple, fuera del corredor. | Abierto · opcional |
| **Banca exterior** | Patio y borde de cancha | Banca acorde al mobiliario escolar, con pivote en el suelo y colisión ajustada fuera de los pasos. | Abierto · opcional |

## Comprobaciones de integración

- La carga directa y el arranque desde `Startup.unity` llevan al mismo punto de aparición en la carretera. La escena tiene un `LevelRoot` con la definición `level-01` y una cámara activa.
- En Play se recorrió la carretera desde la aparición hasta el patio, se entró y salió de ambas aulas y de la sala, se cruzó la cancha y se regresó por la puerta peatonal. Se comprobó la colisión de la reja, un tronco, un pupitre y el muro posterior de la sala.
- Se verificó que la cámara se acerca al jugador cuando la pared de la sala obstruye su posición normal. El mapa admite movimiento con **WASD** o las flechas y movimiento de vista con el ratón. **Esc** libera el cursor y un clic vuelve a capturarlo.
- Unity validó la escena con 344 instancias de 37 prefabs de `ResourcePack`, todas a escala de origen. Se aprobaron 12 pruebas de Edit Mode y 4 de Play Mode. Tras un cierre nativo al cargar desde `Startup`, se desactivó `GPU Resident Drawer` en el perfil URP de PC y se repitieron el arranque y el recorrido completo. [Perfil URP de PC](../Assets/Settings/PC_RPAsset.asset)
- Los puntos `Teacher`, `Caretaker`, `Board`, `Battery`, `PanelPatio`, `PanelRoof` y `RoofAccess` se guardan en `LevelMarkers` como referencias para etapas posteriores. [Plan del mapa](Plan-Mapa-Nivel01.md)
