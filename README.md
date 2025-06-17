# TFG-Vertikal
# Vertikal – Registro digital de ascensiones montañeras (MVP)

Este proyecto es el prototipo funcional desarrollado como parte de un Trabajo Fin de Grado, cuyo objetivo es modernizar el registro de ascensiones a cumbres montañeras mediante una aplicación móvil multiplataforma.

## 📱 Funcionalidades implementadas (MVP)

La primera versión del sistema incluye las siguientes funcionalidades básicas:

- Registro de usuario mediante correo electrónico (Firebase Authentication)
- Consulta del listado de cumbres disponibles (a través de Firestore)
- Registro automático de ascensiones mediante validación por geolocalización GPS
- Consulta del historial personal de ascensiones
- Edición básica del perfil de usuario (modificación del nombre)
- Sincronización en tiempo real con Firebase Firestore (acceso vía API REST)
- Pruebas unitarias sobre la lógica de negocio (Vertikal.Core)

> ⚠️ Esta versión **no incluye** aún validación por QR/NFC, retos, certificados, ni funcionalidades colaborativas (clubes o federaciones).

---

## 🧱 Estructura del repositorio

El proyecto está organizado en tres módulos principales:

- **Vertikal**: aplicación cliente desarrollada con .NET MAUI (UI y navegación)
- **Vertikal.Core**: módulo de lógica de negocio y servicios reutilizables
- **Vertikal.Test**: suite de pruebas unitarias sobre Vertikal.Core

---

## 🛠️ Tecnologías utilizadas

- [.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/) – Framework multiplataforma de UI
- [Firebase Authentication](https://firebase.google.com/docs/auth) – Registro y autenticación de usuarios
- [Firebase Firestore REST API](https://firebase.google.com/docs/firestore/use-rest-api) – Acceso a datos vía API HTTP REST
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) – Entorno de desarrollo principal

---

## 🚀 Requisitos para ejecución

- Tener instalado Visual Studio con el workload de MAUI y Android
- Cuenta de Firebase configurada con:
  - Autenticación por email habilitada
  - Base de datos Firestore activa
- Clonar el repositorio
- Configurar credenciales de acceso:  
  - La configuración de acceso a los servicios de Firebase se gestiona a través del archivo `appSettings.json`, ubicado en el directorio `Resources/Raw`.
  - Por motivos de seguridad, el archivo `appSettings.json` real no está incluido en el repositorio.
  - Se proporciona el archivo `appSettings.example.json` como plantilla de ejemplo, que muestra la estructura necesarias.

---

## 📂 Estado del desarrollo

Este prototipo constituye una primera iteración funcional (MVP) del sistema. El diseño modular permite ampliar fácilmente el proyecto con nuevas funcionalidades en futuras versiones, como:

- Validación por NFC o QR
- Registro sin conexión (modo offline)
- Gestión de retos y certificaciones
- Perfiles colaborativos (clubes, federaciones)
- Backend propio desacoplado de Firebase

---

## 📝 Autor

Xabier Mikel Martín Díaz de Cerio  
Trabajo Fin de Grado – Grado en Ingeniería Informática  
Universidad Internacional de La Rioja (UNIR)

---

## 📄 Licencia

Este proyecto tiene fines académicos y no incluye licencias de distribución pública. El uso de Firebase está sujeto a sus propias condiciones de uso.

