# LC-13: Actividades, Eventos y Reglas Iniciales de Balance — LUZ CERO

- **Identificador:** LC-13
- **Responsable funcional:** Walter
- **Rol:** Level Designer
- **Etapa:** Producción
- **Prioridad:** P0
- **Colaboración:** José y Marco
- **Dependencias:** LC-01 (#1), LC-06 (#6), LC-12 (#12)
- **Nota de balance:** Todos los valores numéricos de este documento son **provisionales** y están sujetos a validación en prototipo junto con José y Marco.

---

## 1. Principio Anti-"Apagar Todo" y Prioridad Educativa

En **LUZ CERO**, el objetivo no es ahorrar energía apagando la escuela entera ni frenando las clases. Operar bajo flujo continuo con prioridad educativa implica sostener las actividades escolares activas sin cortes no planificados ni sobrecargas.

- **Regla anti-apagón total:** Apagar todas las aulas o cortar el suministro general detiene las actividades educativas en curso y penaliza la **Continuidad Educativa**. Obtener una evaluación alta exige mantener activas las tareas escolares dentro del presupuesto y cuidando la reserva de batería.
- **Equilibrio de tres pilares:** Cada decisión se evalúa según **Continuidad Educativa** (que las clases/actividades se cumplan sin interrupciones), **Eficiencia / Ahorro** (uso responsable de créditos y energía sin derroche) y **Estabilidad / Seguridad** (evitar cortes por sobrecarga, descarga profunda o manipular circuitos con tensión).

---

## 2. Actividades Escolares (Lectura, Informática y Presentación Audiovisual)

Las actividades se activan en las zonas definidas en LC-01 (**Aula A**, **Aula B**, **Laboratorio** y **Patio**, apoyadas por la **Sala Eléctrica** y **Techos Solares**) con flujo continuo sin jornadas rígidas obligatorias.

| Actividad | Zona principal | Requisito energético y de entorno *(provisional)* | Adaptación del jugador / sistema | Consecuencia si falta energía o hay fallo |
| :--- | :--- | :--- | :--- | :--- |
| **Lectura** | Aula A (base) / Aula B (apoyo) | Demanda **baja** (`~10 W` / `1.0 u/s` *provisional*); requiere iluminación mínima legible cuando cae la luz natural o hay sombra. | Usar luminaria eficiente en vez de lámpara estándar de alto consumo, o aprovechar luz natural diurna cuando esté disponible. | Dificultad visual y pérdida gradual de ritmo/progreso de lectura; se registra interrupción en el informe si se detiene la actividad. |
| **Informática** | Laboratorio | Demanda **alta** (`~35 W` / `3.5 u/s` *provisional*) con picos al iniciar equipos/sesiones; exige estabilidad de tensión y respaldo ante variaciones. | Escalonar el encendido de equipos, priorizar batería/generación en picos o alternar grupos de trabajo si la generación es limitada. | Corte de sesiones en equipos activos, riesgo de disparo/sobrecarga si el consumo supera el suministro y registro de caída de servicio. |
| **Presentación Audiovisual** | Aula B (o salón multimedia) | Demanda **media-alta** (`~22 W` / `2.2 u/s` *provisional*); sensible a cortes abruptos durante la proyección. | Reducir cargas secundarias no esenciales en otras zonas mientras dura la presentación y verificar respaldo de batería. | Apagón del proyector/audio a mitad de la exposición, interrupción de la actividad y marca de corte en el reporte final. |

---

## 3. Eventos y Situaciones Comprensibles

Los eventos aparecen con señales visuales y sonoras claras para que el jugador identifique la causa y tome decisiones sin adivinar:

1. **Nubosidad (Reducción temporal de captación solar):**
   - **Causa comprensible:** Paso de nubes densas sobre el campus que reduce la generación fotovoltaica (más notoria en zonas con mayor exposición o sombra variable).
   - **Señal y respuesta:** Indicador de reducción de entrada solar y cambio visual en el cielo/iluminación. El jugador puede apoyarse en la **reserva de batería**, priorizar actividades críticas (**Lectura** / **Informática** activa) y postergar cargas prescindibles.
2. **Corte / Sobrecarga o Fallo de Circuito:**
   - **Causa comprensible:** La demanda simultánea supera la capacidad disponible del circuito/línea, o una conexión requiere revisión/aislamiento en la **Sala Eléctrica**.
   - **Señal y respuesta:** Alerta en el tablero/HUD indicando qué zona o línea sufrió el corte. El jugador debe revisar cargas, aislar/rearmar con seguridad (bajar cuchillas y verificar antes de intervenir) y probar la continuidad.
3. **Equipos Olvidados Encendidos:**
   - **Causa comprensible:** Luces, proyector o equipos del laboratorio/aulas quedan encendidos tras finalizar o pausar una actividad, consumiendo energía sin aportar progreso educativo activo.
   - **Señal y respuesta:** Aviso contextual o indicador de consumo ocioso en la zona. El jugador decide si apagar el equipo olvidado para liberar energía y ahorrar créditos, o mantenerlo solo si lo justifica una actividad inmediata.

---

## 4. Valores Iniciales, Indicadores y Criterios de Éxito *(Provisionales)*

### Parámetros iniciales de prueba (ajustables en prototipo)
- **Presupuesto inicial de referencia:** `100` créditos *(provisional)*.
- **Reserva mínima de batería recomendada:** `>= 20 %` al sostener la operación continua *(provisional)*.
- **Consumo de referencia por actividad:** Lectura (`1.0 u/s`), Presentación Audiovisual (`2.2 u/s`), Informática (`3.5 u/s`) *(provisionales)*.
- **Penalización por apagar todo / cortar actividad:** Reducción directa en el puntaje de **Continuidad Educativa** si se detienen las actividades en curso solo para ahorrar energía o evitar cortes.

### Indicadores clave (KPIs del prototipo)
- **% de Continuidad Educativa:** Tiempo o porcentaje de actividades completadas sin interrupciones no planificadas.
- **Energía neta y saldo de créditos:** Créditos restantes y balance entre generación, almacenamiento y consumo real.
- **Reserva de batería y eventos resueltos:** Porcentaje de batería conservado y tiempo/acierto al resolver nubosidad, cortes o equipos olvidados sin incidentes de seguridad.

### Criterios de éxito para validar en pruebas
- **Éxito base (MVP):** Completar el ciclo de actividades solicitadas manteniendo la **Continuidad Educativa** por encima del umbral definido *(p. ej., >= 80 % provisional)*, sin agotar el presupuesto (`100` créditos) y respetando las reglas básicas de seguridad.
- **Condición de fallo / revisión:** Quedarse sin créditos antes de cumplir las tareas, provocar cortes reiterados por sobrecarga/falta de respaldo o intentar lograr "buen puntaje" apagando todos los equipos y dejando sin servicio las actividades.

---

## 5. Progresión de Mejoras (Flujo Continuo Sin Jornadas Obligatorias)

El avance se organiza como una progresión incremental dentro del flujo continuo —sin cortes obligatorios por días o jornadas— donde el jugador desbloquea o elige mejoras según sus necesidades y recursos:

1. **Fase 1 — Estabilización y lectura del sistema:** Comprender el consumo de **Lectura**, **Informática** y **Presentación Audiovisual**, controlar interruptores/cuchillas en la **Sala Eléctrica** y atender **equipos olvidados**.
2. **Fase 2 — Respaldo y respuesta a variaciones:** Gestionar la carga/descarga de batería y ajustar el uso ante **nubosidad** y **cortes** sin detener la actividad principal.
3. **Fase 3 — Optimización y ampliación:** Incorporar mejoras de eficiencia (p. ej., luminarias de menor consumo, mejor captación en techos/patio o mayor capacidad de almacenamiento) para sostener mayor actividad simultánea con menor costo operativo.
