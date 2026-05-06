# **Documento de Especificaciones Técnicas: Feedy \- Mobile PWA**

## **1\. Resumen Ejecutivo (Project Overview)**

El presente documento detalla los requerimientos exhaustivos para el desarrollo de la interfaz de usuario (Frontend) de la plataforma **Feedy**. La solución se concebirá como una **Progressive Web App (PWA)** bajo el paradigma de diseño **"Mobile-First"**, garantizando una experiencia nativa en navegadores móviles. El objetivo primordial de **Feedy** radica en proporcionar una experiencia de usuario optimizada y fluida, caracterizada por una estética sofisticada de nivel profesional y un sistema de recomendación algorítmico avanzado denominado "Smart Feed". Este motor está orientado al descubrimiento personalizado de contenido gastronómico, transformando la búsqueda pasiva en una oferta activa de valor basada en el perfil psicográfico y nutricional del usuario.

## **2\. Pila Tecnológica (Tech Stack)**

Para garantizar la escalabilidad, la mantenibilidad y un rendimiento excepcional del sistema, se han seleccionado las siguientes tecnologías de vanguardia:

* **Framework de Desarrollo:** React.js versión 18+ optimizado mediante Vite, utilizando TypeScript para asegurar la integridad de tipos.  
* **Estilización y Diseño:** Tailwind CSS como motor de utilidades, fundamentado en una configuración de `tailwind.config.js` que implemente variables de diseño (tokens).  
* **Tipografía (Branding):** \* **Titulares:** [Bitter](https://fonts.google.com/specimen/Bitter) (Serif) vía Google Fonts para aportar carácter, elegancia y una lectura pausada en títulos.  
  * **Cuerpo de texto:** Sans-serif de alta legibilidad (v.g., Inter o Montserrat).  
* **Sistemas de Iconografía:** Uso combinado de Lucide-React para elementos funcionales y Phosphor Icons para elementos decorativos.  
* **Gestión de Estado y Sincronización:** Empleo de React Context API para estados globales y TanStack Query para la sincronización con el servicio de Backend desarrollado en .NET 8\.  
* **Librerías de Componentes:** Implementación de componentes de bajo nivel como Headless UI o Radix UI.

  ## **3\. Funcionalidades Principales (Core Features \- MVP)**

  ### **3.1 Onboarding y Gestión de Perfiles**

* **Interfaz de Bienvenida Dinámica:** Proceso de inducción que incluye la selección de preferencias mediante componentes visuales de alta fidelidad, permitiendo la discriminación entre perfiles de sabor (v.g., preferencia por perfiles salados frente a dulces).  
* **Configuración de Restricciones Alimentarias Críticas:** Módulo de búsqueda predictiva para la identificación inmediata de "Ingredientes Rechazados". Gestión de exclusiones mediante una interfaz de selección múltiple basada en etiquetas (tags/chips).  
* **Definición de Régimen y Estilo de Vida:** Selección de categorías (v.g., Vegano, Keto, Celíaco, Paleo) que condicionarán dinámicamente el contenido de **Feedy**.

  ### **3.2 Lógica del Feed Inteligente (Smart Feed)**

* **Indicadores de Afinidad Visual Proactiva:** Cada receta en el feed deberá integrar un distintivo visual (badge) que represente el grado de compatibilidad ("Match Percentage") calculado por el motor de **Feedy**.  
* **Arquitectura de Secuencias Curadas:** Implementación del componente de "Suscripción a Secuencias". Este módulo permite visualizar una cronología de ingestas programadas para períodos determinados.  
* **Mecanismos de Interacción Adaptativos:** Integración de gestos táctiles avanzados (swiping) para acciones rápidas de aprobación o descarte.

  ### **3.3 Visualización Detallada de Recetas**

* **Modo de Lectura "Hands-Free" Optimizado:** Interfaz adaptada para entornos de cocina, priorizando la legibilidad a distancia y la prevención del bloqueo de pantalla.  
* **Gestión Interactiva de Ingredientes:** Listado de verificación funcional (checklist) para el seguimiento de la preparación en tiempo real.  
* **Ficha Técnica Detallada:** Representación iconográfica clara de complejidad, tiempos y valores nutricionales clave.

  ## **4\. Directrices de UI/UX (Designer Perspective)**

* **Sistema de Diseño Orgánico:** Aplicación estricta de una paleta cromática fundamentada en la paleta de colores en el documento "color-pallete.png" incorporando + naranja con FFAA00
* **Jerarquía Tipográfica:** \* **Títulos:** Uso obligatorio de **Bitter** en distintos pesos para establecer una jerarquía editorial sofisticada.  
  * **Cuerpo:** Fuentes Sans-serif que aseguren la máxima legibilidad en dispositivos móviles.  
* **Micro-interacciones:** Implementación de retroalimentación visual y háptica tras la ejecución de acciones críticas.  
* **Adaptabilidad:** Restricción sistemática del ancho máximo del contenedor principal en terminales de escritorio (max-w-md) para preservar la ergonomía del formato móvil.

  ## **5\. Arquitectura y Requerimientos Técnicos de Calidad**

* **Patrón de Arquitectura Modular:** Estructura de directorios organizada por funcionalidades (Feature-based folder structure).  
* **Integración de Servicios:** Cliente de API centralizado destinado exclusivamente al consumo del backend desarrollado en .NET 8\.  
* **Capacidades de PWA:** Configuración de `manifest.json` y Service Workers para habilitar la persistencia de datos en modo offline y acceso rápido desde la pantalla de inicio.  
* **Optimización de Performance:** Carga diferida (Lazy loading) de activos multimedia y empleo de Skeletons diseñados a medida para evitar el desplazamiento de diseño (Layout Shift).

  ## **6\. Escenarios de Usuario y Casos de Uso (User Stories)**

* *En calidad de usuario,* deseo que **Feedy** filtre automáticamente recetas con ingredientes rechazados para garantizar mi seguridad alimentaria.  
* *En calidad de usuario,* quiero suscribirme a una secuencia dietética para que mi feed diario se organice sin intervención manual.  
* *En calidad de usuario,* deseo etiquetar contenidos para que el motor de **Feedy** aprenda de mis hábitos y mejore las sugerencias futuras.

  ## **7\. Contratos de Interfaz y Estructura de Datos (API Expectations)**

La interfaz de usuario de **Feedy** requiere objetos con la siguiente estructura de datos mínima:

* /\*\*  
*  \* Representación del objeto de receta optimizado para el Smart Feed de Feedy  
*  \*/  
* interface Recipe {  
*   id: string; // Identificador único universal (UUID)  
*   title: string; // Título descriptivo (Renderizado con tipografía Bitter)  
*   description: string; // Resumen ejecutivo del plato  
*   matchPercentage: number; // Valor flotante (0-100) de afinidad  
*   tags: string\[\]; // Colección de etiquetas nutricionales  
*   difficulty: 'Easy' | 'Medium' | 'Hard'; // Clasificación de complejidad  
*   prepTimeMinutes: number; // Tiempo estimado  
*   isSubscribedSequence: boolean; // Flag de pertenencia a secuencia activa  
*   mainImageUrl: string; // URL de la imagen principal  
* }  
    
* 
