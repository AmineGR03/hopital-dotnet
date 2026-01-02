# 📋 Guide d'installation et de configuration

Ce guide vous explique en détail comment cloner, configurer et lancer le projet **Gestion Hôpital** depuis GitHub.

---

## 📦 Prérequis

Avant de commencer, assurez-vous d'avoir installé les outils suivants :

### 1. **.NET SDK 9.0**
   - Téléchargez depuis : https://dotnet.microsoft.com/download/dotnet/9.0
   - Vérifiez l'installation :
     ```bash
     dotnet --version
     ```
     Vous devriez voir `9.0.x` ou supérieur.

### 2. **SQL Server**
   - **SQL Server Express** (gratuit) : https://www.microsoft.com/sql-server/sql-server-downloads
   - **SQL Server LocalDB** (recommandé pour le développement) : Inclus avec Visual Studio
   - Vérifiez l'installation :
     ```bash
     sqllocaldb info
     ```

### 3. **Git**
   - Téléchargez depuis : https://git-scm.com/downloads
   - Vérifiez l'installation :
     ```bash
     git --version
     ```

### 4. **Visual Studio 2022** (optionnel mais recommandé)
   - Téléchargez depuis : https://visualstudio.microsoft.com/
   - Assurez-vous d'installer la charge de travail **"Développement web et ASP.NET"**

---

## 🔽 Étape 1 : Cloner le projet depuis GitHub

### Option A : Via la ligne de commande

1. **Ouvrez PowerShell ou l'invite de commandes**

2. **Naviguez vers le dossier où vous voulez cloner le projet**
   ```powershell
   cd C:\Users\VotreNom\Desktop
   ```

3. **Clonez le dépôt**
   ```powershell
   git clone https://github.com/AmineGR03/hopital-dotnet.git
   ```

4. **Naviguez dans le dossier du projet**
   ```powershell
   cd hopital-dotnet\gestion_hopital
   ```

### Option B : Via Visual Studio

1. Ouvrez **Visual Studio 2022**
2. Cliquez sur **"Cloner un dépôt"**
3. Entrez l'URL : `https://github.com/AmineGR03/hopital-dotnet.git`
4. Choisissez un dossier de destination
5. Cliquez sur **"Cloner"**

---

## 🗄️ Étape 2 : Configuration de la base de données

### 2.1. Vérifier/créer SQL Server LocalDB

1. **Ouvrez PowerShell en tant qu'administrateur**

2. **Vérifiez si LocalDB est installé**
   ```powershell
   sqllocaldb info
   ```

3. **Si LocalDB n'est pas installé**, il sera installé automatiquement avec Visual Studio, ou vous pouvez l'installer via SQL Server Express.

4. **Créez une instance LocalDB (si nécessaire)**
   ```powershell
   sqllocaldb create "MSSQLLocalDB"
   ```

5. **Démarrez l'instance**
   ```powershell
   sqllocaldb start "MSSQLLocalDB"
   ```

### 2.2. Configurer la chaîne de connexion

1. **Ouvrez le fichier `appsettings.json`** dans le dossier `gestion_hopital`

2. **Modifiez la chaîne de connexion** selon votre configuration :

   **Pour SQL Server LocalDB (recommandé pour le développement) :**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GestionHopitalDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

   **Pour SQL Server Express (si vous utilisez SQL Server Express) :**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.\\SQLEXPRESS;Database=GestionHopitalDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     },
     ...
   }
   ```

   **Pour SQL Server avec authentification SQL :**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=VOTRE_SERVEUR;Database=GestionHopitalDB;User Id=VOTRE_UTILISATEUR;Password=VOTRE_MOT_DE_PASSE;TrustServerCertificate=True"
     },
     ...
   }
   ```

3. **Sauvegardez le fichier**

---

## 🔧 Étape 3 : Restaurer les dépendances et créer la base de données

### 3.1. Restaurer les packages NuGet

1. **Ouvrez PowerShell dans le dossier `gestion_hopital`**

2. **Restorez les packages NuGet**
   ```powershell
   dotnet restore
   ```

   Cette commande télécharge tous les packages nécessaires listés dans `gestion_hopital.csproj`.

### 3.2. Créer la base de données

1. **Vérifiez que votre SQL Server/LocalDB est en cours d'exécution**

2. **Créez la base de données et appliquez les migrations**
   ```powershell
   dotnet ef database update
   ```

   **Note :** Si vous obtenez une erreur indiquant que `dotnet ef` n'est pas reconnu, installez les outils EF Core :
   ```powershell
   dotnet tool install --global dotnet-ef
   ```

   Ensuite, réessayez :
   ```powershell
   dotnet ef database update
   ```

   Cette commande va :
   - Créer la base de données si elle n'existe pas
   - Appliquer toutes les migrations pour créer les tables
   - Créer le schéma de base de données complet

---

## 🚀 Étape 4 : Lancer l'application

### Option A : Via la ligne de commande

1. **Dans PowerShell, depuis le dossier `gestion_hopital`**
   ```powershell
   dotnet run
   ```

2. **L'application démarrera et sera accessible sur :**
   - HTTP : `http://localhost:6000`
   - HTTPS : `https://localhost:6001`

3. **Ouvrez votre navigateur et accédez à l'URL affichée dans la console**

### Option B : Via Visual Studio

1. **Ouvrez le fichier `gestion_hopital.sln`** dans Visual Studio
   - Ou ouvrez le dossier `gestion_hopital` comme projet

2. **Assurez-vous que le projet de démarrage est configuré**
   - Clic droit sur le projet → **"Définir comme projet de démarrage"**

3. **Appuyez sur `F5`** ou cliquez sur le bouton **"Démarrer"**

4. **L'application s'ouvrira dans votre navigateur par défaut**

---

## 🌱 Étape 5 : Initialisation des données (Seeder)

**Important :** Le seeder s'exécute **automatiquement** au premier démarrage de l'application.

Il crée automatiquement :
- ✅ Les rôles (admin, doctor, receptionist)
- ✅ Les utilisateurs de test (voir `IDENTIFIANTS.txt`)
- ✅ Les données de démonstration (médecins, patients, rendez-vous, prescriptions)

**Aucune action manuelle n'est nécessaire !**

Les identifiants de connexion sont disponibles dans le fichier `IDENTIFIANTS.txt` à la racine du projet.

---

## 🔑 Identifiants de connexion

Après le premier démarrage, vous pouvez vous connecter avec :

### Administrateur
- **Email :** `admin@hopital.com`
- **Mot de passe :** `Admin123!`
- **Accès :** Toutes les fonctionnalités

### Médecin 1
- **Email :** `doctor@hopital.com`
- **Mot de passe :** `Doctor123!`
- **Accès :** Patients, rendez-vous, prescriptions

### Médecin 2
- **Email :** `doctor2@hopital.com`
- **Mot de passe :** `Doctor2123!`
- **Accès :** Patients, rendez-vous, prescriptions

### Réceptionniste
- **Email :** `receptionist@hopital.com`
- **Mot de passe :** `Receptionist123!`
- **Accès :** Médecins, patients, rendez-vous

📄 **Voir le fichier `IDENTIFIANTS.txt` pour plus de détails.**

---

## ⚠️ Dépannage

### Problème : "Cannot open database"

**Solution :**
1. Vérifiez que SQL Server/LocalDB est en cours d'exécution
2. Vérifiez la chaîne de connexion dans `appsettings.json`
3. Réessayez `dotnet ef database update`

### Problème : "dotnet ef" n'est pas reconnu

**Solution :**
```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update
```

### Problème : Port déjà utilisé

**Solution :**
1. Modifiez les ports dans `Properties/launchSettings.json`
2. Ou arrêtez l'application qui utilise le port 6000/6001

### Problème : Erreurs de migration

**Solution :**
1. Supprimez la base de données existante
2. Réexécutez : `dotnet ef database update`

### Problème : Packages NuGet manquants

**Solution :**
```powershell
dotnet restore
dotnet build
```

---

## 📁 Structure du projet

```
gestion_hopital/
├── Areas/
│   └── Identity/          # Pages d'authentification
├── Controllers/           # Contrôleurs MVC
├── Data/                  # DbContext et migrations
│   └── Migrations/        # Migrations Entity Framework
├── Models/                # Modèles de données
├── Views/                 # Vues Razor
├── wwwroot/              # Fichiers statiques (CSS, JS)
├── appsettings.json      # Configuration (chaîne de connexion)
├── Program.cs            # Point d'entrée de l'application
└── gestion_hopital.csproj # Fichier de projet
```

---

## 🔄 Mettre à jour le projet

Pour récupérer les dernières modifications depuis GitHub :

```powershell
git pull origin main
dotnet restore
dotnet ef database update
dotnet run
```

---

## 📝 Notes importantes

1. **Base de données :** La base de données est créée automatiquement au premier démarrage grâce aux migrations Entity Framework.

2. **Seeder :** Les données de test sont créées automatiquement au premier démarrage. Le seeder ne recrée pas les données si elles existent déjà.

3. **Mots de passe :** Tous les mots de passe de test respectent les exigences de sécurité ASP.NET Identity (minimum 6 caractères, avec majuscule, minuscule et caractère spécial).

4. **Ports :** Par défaut, l'application utilise les ports 6000 (HTTP) et 6001 (HTTPS). Vous pouvez les modifier dans `Properties/launchSettings.json`.

5. **Environnement de développement :** Le fichier `appsettings.Development.json` contient des paramètres spécifiques au développement.

---

## ✅ Vérification finale

Avant de commencer à utiliser l'application, vérifiez que :

- ✅ .NET SDK 9.0 est installé
- ✅ SQL Server/LocalDB est installé et en cours d'exécution
- ✅ La chaîne de connexion est correctement configurée dans `appsettings.json`
- ✅ Les migrations ont été appliquées (`dotnet ef database update` a réussi)
- ✅ L'application démarre sans erreur
- ✅ Vous pouvez vous connecter avec l'un des identifiants de test

---

## 🆘 Support

Si vous rencontrez des problèmes :

1. Vérifiez que tous les prérequis sont installés
2. Consultez la section **Dépannage** ci-dessus
3. Vérifiez les logs dans la console pour les erreurs détaillées
4. Assurez-vous que la base de données est accessible

---

**Bon développement ! 🎉**

