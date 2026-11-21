# LC-06: Especificación de Blockout y Validación Espacial — Escuela Rural

- **Identificador:** LC-06
- **Responsable funcional:** Walter
- **Rol:** Level Designer
- **Etapa:** Preproducción
- **Prioridad:** P0
- **Colaboración:** Elias y José
- **Dependencias:** LC-01 (#1), LC-05 (#5)

---

## 1. Objetivo y Alcance del Blockout

Validar la distribución espacial, proporciones, accesos, recorridos y legibilidad visual del campus rural de **LUZ CERO** mediante geometría provisional (*greyboxing* en Unity) antes de iniciar el modelado 3D definitivo.

- **Escena de trabajo:** `Assets/Scenes/Blockout_CampusRural.unity` (escena independiente para no alterar escenas activas del equipo).
- **Zonas incluidas en el blockout:**
  - **Aula A:** Salón base de baja demanda energética.
  - **Aula B:** Salón complementario de demanda media.
  - **Laboratorio:** Espacio técnico de alta demanda y picos de consumo.
  - **Patio:** Eje central de circulación, recreo y referencia de captación exterior.
  - **Sala Eléctrica:** Núcleo de control (tablero principal, cuchillas de aislamiento, banco de baterías y puntos de verificación).
  - **Techos Solares:** Superficies delimitadas en cubierta (y opción en patio) para la ubicación de paneles fotovoltaicos.

---

## 2. Estructura de Jerarquía y Convención Visual en Unity

Para facilitar la revisión con **Elias** (arte 3D) y **José** (sistemas/programación), el blockout se organiza bajo los siguientes contenedores en la jerarquía:

- `01_Terreno_y_Patio`: Suelo base del campus (`30 m × 25 m` aprox.) y senderos principales de tránsito.
- `02_Edificios`: Volúmenes provisionales de **Aula A**, **Aula B**, **Laboratorio**, **Sala Eléctrica** y plataformas de **Techos Solares**.
- `03_Puntos_Interaccion`: Primitivas codificadas por color según su función jugable:
  - **Azul / Verde:** Puntos de interacción del jugador (tablero eléctrico, cuchillas, caja de conexiones, puntos de medición con multímetro).
  - **Amarillo:** Paneles solares y banco de baterías.
  - **Naranja:** Cargas eléctricas de consumo (luminarias por aula, proyector, equipos de laboratorio).
- `04_Recorridos_y_Spawns`: Cápsulas de referencia (`1.8 m` de altura) para validar ancho de puertas/pasillos (mínimo `2.0 m`–`2.5 m`) y rutas de desplazamiento de jugador y NPCs.
- `05_Camara_e_Iluminacion`: Configuración de cámara isométrica e iluminación base de prueba.

---

## 3. Criterios de Revisión Isométrica (Encuadres y Oclusiones)

- **Visibilidad interior:** Los muros frontales respecto al ángulo de la cámara isométrica mantienen altura reducida (*cutaway* / vista abierta) para evitar que tapen al jugador, los tableros o el mobiliario interior de las aulas y la sala eléctrica.
- **Legibilidad de zonas:** Cada edificio conserva proporciones y accesos diferenciados desde el patio central para reconocerse a primera vista sin depender todavía de texturas finales.
- **Accesibilidad:** Todos los puntos de interacción (tablero, cuchillas, paneles, interruptores) cuentan con espacio libre frontal para acercarse y operar sin colisiones bloqueantes.

---

## 4. Criterios de Finalización y Validación con el Equipo

- [ ] Escena `Blockout_CampusRural.unity` navegable en modo *Play* dentro de Unity.
- [ ] Cinco zonas principales + techos solares diferenciados y reconocibles desde cámara isométrica.
- [ ] Puntos de interacción, dispositivos y recorridos básicos situados sin oclusiones críticas.
- [ ] Revisión y aprobación conjunta con **Elias** y **José** antes de pasar a producción artística definitiva.
