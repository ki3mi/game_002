# EcoElectry

EcoElectry es un juego de puzles de construcción y simulación en 3D low poly. Eres un electricista de un programa municipal de electrificación. Empiezas instalando energía solar en un colegio rural y avanzas hacia encargos más complejos, hasta abastecer los servicios de una ciudad con energía renovable.

El diseño aborda el **ODS 7** mediante decisiones sobre generación renovable, eficiencia y continuidad del servicio, y el **ODS 4** mediante prácticas guiadas con herramientas eléctricas.

## Cómo se juega

- Recorre cada establecimiento, conversa con sus responsables y reúne los requisitos del encargo.
- Construye con piezas encajables, elige los recorridos del cableado y pasa a primera persona para conectar y verificar equipos.
- Simula una jornada para comprobar el suministro. Si un servicio falla, usa el informe para corregir el diseño y volver a probar.
- Gana estrellas según los requisitos, el presupuesto y el objetivo de excelencia. Usa los créditos de campaña para comprar mejoras y vuelve a encargos anteriores para conseguir mejores resultados y potenciadores.

## Abrir el proyecto

1. Si obtuviste el repositorio mediante Git y faltan los archivos binarios de Git LFS, ejecuta `git lfs pull` en la raíz.
2. En Unity Hub, agrega la carpeta raíz y abre el proyecto con **Unity `6000.6.0f1`**, la versión registrada en [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
3. En el Editor, abre [`SampleScene.unity`](Assets/Scenes/SampleScene.unity), la escena registrada en la configuración de compilación, y pulsa **Play** para revisar el estado actual del proyecto.

El recorrido previsto para el primer nivel, *La primera aula*, está descrito en su documento de diseño.

## Documentación

| Documento | Contenido |
| --- | --- |
| [Contexto](Docs/Context.md) | Premisa, identidad visual, ODS y progresión de la campaña. |
| [Mecánicas](Docs/Mecanicas.md) | Construcción, cableado, simulación, estrellas, economía y habilidades. |
| [Nivel 01 · La primera aula](Docs/Nivel01-LaPrimeraAula.md) | Diálogos, tareas del tutorial, prueba y criterios de entrega. |
| [Arquitectura](Docs/Arquitectura.md) | Organización propuesta del código, escenas, datos, prefabs y modelos 3D. |
