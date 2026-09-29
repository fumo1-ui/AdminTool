# WinPE & Windows User & System Admin Suite (WinPE_AdminTool)

[Русский](#русский) | [English](#english)

---

## Русский

Универсальная утилита с графическим интерфейсом (WinForms) для управления учетными записями, правами доступа, ограничениями и файловой системой в среде **стандартной Windows** и **Windows PE (WinRE / восстановительные диски)**.

### 🚀 Основные возможности

#### 1. 👤 Управление учетными записями и правами
- **Выдача прав Администратора:** Добавление выбранного пользователя в локальную группу `Administrators`.
- **Снятие прав Администратора:** Удаление пользователя из группы `Administrators`.
- **Снятие всех ограничений (Unrestrict):** 
  - Активация заблокированных и отключенных аккаунтов (`/active:yes`).
  - Снятие срока действия паролей и учетной записи (`/expires:never`).
  - Разрешение на самостоятельное изменение пароля.
- **Создание пользователей:** Создание новых локальных учетных записей с возможностью сразу задать пароль и административные права.
- **Удаление пользователей:** Полное удаление учетных записей из системы.

#### 2. 💽 Выбор целевого диска
- Автоматическое обнаружение всех подключенных дисков и разделов (включая внешние USB-накопители).
- Поддержка работы как с **активной операционной системой**, так и с **офлайн-системами Windows** (при загрузке с WinPE/WinRE флешки).
- Проверка наличия установленной системы Windows на выбранном диске.

#### 3. 📁 Встроенный файловый менеджер (File Explorer)
- Дерево каталогов и список файлов с подробной информацией (размер, тип, дата изменения).
- **Захват прав владения и разблокировка доступа (Take Ownership):** Быстрый сброс ограничений доступа (ACL) к папкам и файлам офлайн-систем с помощью утилит `takeown` и `icacls`.
- Быстрое открытие и удаление файлов прямо из интерфейса.

#### 4. 📊 Менеджер дисков и накопителей
- Просмотр всех логических дисков, их файловых систем (NTFS, FAT32 и др.), общего и свободного дискового пространства.

---

### 🛠 Сборка и запуск

Приложение скомпилировано в **единый автономный файл `.exe` (Single-File Self-Contained Executable)**. Оно содержит внутри себя все необходимые библиотеки и рантайм .NET, поэтому **не требует предварительной установки .NET SDK или Framework** на целевом ПК или в среде WinPE.

#### Путь к готовому файлу EXE:
```text
C:\Users\CatRebornGit\.gemini\antigravity\scratch\WinPE-AdminTool\publish\WinPE_AdminTool.exe
```

#### Пересборка из исходников:
Если вы хотите внести изменения в код и пересобрать проект самостоятельно:

1. Откройте консоль в директории проекта:
   ```cmd
   cd C:\Users\CatRebornGit\.gemini\antigravity\scratch\WinPE-AdminTool
   ```
2. Выполните команду публикации в один EXE-файл:
   ```cmd
   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o "./publish"
   ```

---

### 📋 Требования
- **ОС:** Windows 7 / 8.1 / 10 / 11, Windows Server, или Windows PE / WinRE (x64).
- **Права:** Запуск от имени Администратора (в файл `app.manifest` уже встроено требование `requireAdministrator`).

---

## English

A comprehensive Windows Forms GUI utility designed to manage user accounts, privileges, restrictions, drive access, and file systems across both **Live Windows** environments and **Windows PE / WinRE (Recovery Environments)**.

### 🚀 Key Features

#### 1. 👤 User Account & Privilege Management
- **Grant Administrative Privileges:** Instantly add any selected user account to the local `Administrators` group.
- **Revoke Administrative Privileges:** Remove a user from the `Administrators` group.
- **Remove All Restrictions (Unrestrict):** 
  - Enable disabled or locked-out accounts (`/active:yes`).
  - Clear account and password expiration requirements (`/expires:never`).
  - Allow user-driven password modification.
- **Create User Account:** Interactive dialog to create new local users with optional password setup and admin rights assignment.
- **Delete User Account:** Safely remove unwanted user accounts from the host OS.

#### 2. 💽 Target Drive Selection
- Automatic enumeration of attached storage devices (Internal Disks, External HDDs, USB Drives).
- Supports operating on the **Active Live System** as well as **Offline Windows Installations** mounted in WinPE/WinRE environments.
- Instant detection of Windows system root folders on mounted drives.

#### 3. 📁 Built-in File Explorer
- Directory tree navigation and file listing with attributes (File Size, Type, Date Modified).
- **Take Ownership & Reset Permissions:** One-click resolution (`takeown` and `icacls`) to unlock files/folders blocked by restricted NTFS Access Control Lists (ACLs) in offline or system directories.
- Direct file launch and delete operations.

#### 4. 📊 Storage & Disk Manager
- Overview of all attached volume drives, file system types (NTFS, FAT32), total storage, and available free space.

---

### 🛠 Compilation & Usage

The application is built as a **Single-File Self-Contained Executable (`.exe`)**. It packages the .NET runtime and all binary dependencies internally, requiring **no pre-installed .NET SDK or Framework** on the target computer or WinPE boot disk.

#### Path to Published Executable:
```text
C:\Users\CatRebornGit\.gemini\antigravity\scratch\WinPE-AdminTool\publish\WinPE_AdminTool.exe
```

#### Rebuilding from Source:
To modify source files and recompile the project manually:

1. Open PowerShell / Command Prompt in the project folder:
   ```cmd
   cd C:\Users\CatRebornGit\.gemini\antigravity\scratch\WinPE-AdminTool
   ```
2. Execute the single-file publish command:
   ```cmd
   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o "./publish"
   ```

---

### 📋 System Requirements
- **OS:** Windows 7 / 8.1 / 10 / 11, Windows Server, or Windows PE / WinRE (x64).
- **Privileges:** Administrator privileges required (`app.manifest` includes `requireAdministrator`).
