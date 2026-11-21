# LC-24: Plan de Pruebas de Juego, Balance y Aceptación del MVP — LUZ CERO

- **Identificador:** LC-24
- **Responsable funcional:** Walter
- **Rol:** Level Designer
- **Etapa:** Integración y validación
- **Prioridad:** P0
- **Colaboración:** Todo el equipo (ajuste de balance con José)
- **Dependencias:** LC-21 (#21), LC-22 (#22), LC-23 (#23)
- **Nota:** Los valores y registros iniciales se completan y refinan durante las sesiones de prueba con el prototipo integrado.

---

## 1. Objetivo de Validación

Comprobar que el MVP de **LUZ CERO** enseña la gestión energética a través de decisiones comprensibles en flujo continuo, mantiene un desafío equilibrado y cumple los criterios de prioridad educativa antes del cierre de validación.

---

## 2. Escenarios de Prueba Obligatorios

Cada sesión debe cubrir estos escenarios y verificar su comportamiento visible, energético y educativo:

1. **Lectura con luz natural:** Validar que aprovechar la luz natural reduce la necesidad de iluminación artificial y comprobar cómo reacciona la actividad de lectura ante cambios de iluminación.
2. **Informática (Laboratorio):** Comprobar el consumo alto y los picos al iniciar/operar equipos, la estabilidad del suministro y la respuesta de la actividad si falta energía o hay sobrecarga.
3. **Presentación audiovisual:** Verificar el consumo asociado, la sensibilidad ante cortes o caídas de tensión y el registro de interrupción si se corta el suministro.
4. **Nubosidad:** Observar la reducción temporal de captación solar, el apoyo de la batería y la claridad de las señales para que el jugador reaccione a tiempo.
5. **Corte / fallo de suministro:** Confirmar que los cortes afectan realmente al suministro de las zonas/líneas correspondientes y que el jugador entiende cómo diagnosticar, aislar con seguridad y recuperar el servicio.
6. **Equipos olvidados encendidos:** Verificar que los dispositivos que quedan encendidos sin actividad activa generan un consumo/costo perceptible y comprensible, sin obligar a apagar toda la escuela.

---

## 3. Puntos de Observación en Jugabilidad

Durante las pruebas con jugadores o compañeros, registrar qué tan claro resulta:

- **Controles e interacción:** Facilidad para navegar el campus, alternar cámaras, operar interruptores/cuchillas, medir/conectar y responder a alertas.
- **Prioridades e indicadores:** Si el jugador entiende qué zonas/actividades tienen prioridad educativa, cómo leer el HUD/indicadores de energía, batería y presupuesto, y qué significa cada aviso.
- **Consecuencias y regla anti-"apagar todo":** Confirmar que **apagar todos los equipos o cortar todo no otorga automáticamente la mejor evaluación**, ya que sacrifica la continuidad educativa; asimismo, verificar que la batería respeta sus límites mínimos/máximos y que las clases reaccionan de forma coherente ante la falta de energía.

---

## 4. Bitácora de Sesiones y Problemas por Prioridad

### Registro de sesiones de prueba
| Sesión # | Fecha / Build | Participante / Tester | Escenarios probados | Observaciones clave |
| :--- | :--- | :--- | :--- | :--- |
| Sesión 1 | *Pendiente* | *Pendiente* | Lectura, Informática, Audiovisual | *Completar tras la primera corrida en prototipo* |
| Sesión 2 | *Pendiente* | *Pendiente* | Nubosidad, Corte, Equipos olvidados | *Completar tras probar eventos y fallos* |

### Problemas detectados (clasificación por prioridad)
- **P0 (Bloqueante / Crítico para el MVP):** *Ej. Cortes que no afectan el suministro, batería sin límites o reglas que premien apagar todo.*
- **P1 (Alto / Afecta claridad o balance):** *Ej. Dificultad para notar equipos olvidados, costos o tiempos de carga/descarga desbalanceados en pruebas con José.*
- **P2 (Medio / Mejora de UX o pulido):** *Ej. Ajustes menores de encuadre, textos de ayuda o detalles visuales secundarios.*

---

## 5. Ajustes de Balance con José y Criterios de Aceptación del MVP

### Tabla de seguimiento de ajustes de balance
| Parámetro | Valor inicial *(provisional)* | Valor ajustado *(tras pruebas)* | Motivo del ajuste (con José) |
| :--- | :--- | :--- | :--- |
| Capacidad y límites de batería | *Según LC-13 / prototipo* | *Pendiente* | *Completar tras validar carga/descarga* |
| Frecuencia e intensidad de nubosidad / cortes | *Según LC-13 / prototipo* | *Pendiente* | *Completar según ritmo y desafío observado* |
| Costos, mejoras y consumo por actividad | *Según LC-13 / prototipo* | *Pendiente* | *Completar según ahorro vs. continuidad* |

### Criterios de aceptación del MVP (revisión con todo el equipo)
- [ ] Batería con límites operativos respetados y visibles.
- [ ] Cortes de energía reflejados de forma real y comprensible en el suministro y en las clases.
- [ ] Reglas de evaluación verificadas: ahorrar sin sostener actividades educativas no maximiza automáticamente el resultado.
- [ ] Problemas clasificados por prioridad (`P0`, `P1`, `P2`), ajustes de balance acordados con **José** y pendientes finales documentados y revisados por todo el equipo.

