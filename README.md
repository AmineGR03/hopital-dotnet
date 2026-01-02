# Système de Gestion d'Hôpital

Application ASP.NET Core 9.0 pour la gestion d'un hôpital avec authentification par rôles.

## 🚀 Démarrage rapide

### Prérequis
- .NET 9.0 SDK
- SQL Server (localhost)
- Base de données `gestion_hopital` créée

### Démarrer l'application

1. **Arrêter les processus existants** (si nécessaire) :
```powershell
taskkill /F /IM gestion_hopital.exe
```

2. **Démarrer l'application** :
```powershell
cd gestion_hopital
dotnet run
```

3. **Accéder à l'application** :
   - Ouvrir votre navigateur sur `http://localhost:6000` (HTTP)
   - Ou `https://localhost:6001` (HTTPS)

## 🔑 Identifiants de connexion

### ADMINISTRATEUR
- **Email** : `admin@hopital.com`
- **Mot de passe** : `Admin123!`
- **Accès** : Toutes les fonctionnalités (médecins, patients, rendez-vous, prescriptions)

### MÉDECIN
- **Email** : `doctor@hopital.com`
- **Mot de passe** : `Doctor123!`
- **Accès** : Patients, rendez-vous, prescriptions

### RÉCEPTIONNISTE
- **Email** : `receptionist@hopital.com`
- **Mot de passe** : `Receptionist123!`
- **Accès** : Médecins, patients, rendez-vous

## 📋 Données de test

Le seeder crée automatiquement au démarrage :
- ✅ 3 médecins (Cardiologie, Pédiatrie, Neurologie)
- ✅ 4 patients avec informations complètes
- ✅ 4 rendez-vous avec différents statuts
- ✅ 3 prescriptions avec médicaments
- ✅ Historiques médicaux avec diagnostics passés
- ✅ Relations médecin-patient (many-to-many)

## 🛠️ Résolution de problèmes

### Port déjà utilisé (6000 ou 6001)

Si vous obtenez l'erreur "address already in use" :

```powershell
# Trouver le processus utilisant le port
netstat -ano | findstr :6000
netstat -ano | findstr :6001

# Arrêter le processus (remplacer PID par le numéro trouvé)
taskkill /F /PID <PID>

# Ou arrêter tous les processus gestion_hopital
taskkill /F /IM gestion_hopital.exe
```

**Note** : Les ports peuvent être modifiés dans `Properties/launchSettings.json` si nécessaire.

### Fichier verrouillé lors du build

Si le build échoue avec "file is locked" :

```powershell
# Arrêter tous les processus gestion_hopital
taskkill /F /IM gestion_hopital.exe

# Attendre quelques secondes puis rebuilder
Start-Sleep -Seconds 2
dotnet build
```

## 📁 Structure du projet

```
gestion_hopital/
├── Controllers/          # Contrôleurs MVC
│   ├── DoctorsController.cs
│   ├── PatientsController.cs
│   ├── AppointmentsController.cs
│   └── PrescriptionsController.cs
├── Models/               # Modèles de données
│   ├── ApplicationUser.cs
│   ├── Doctor.cs
│   ├── Patient.cs
│   ├── Appointment.cs
│   ├── Prescription.cs
│   ├── HistoriqueMedical.cs
│   └── DiagnosticPasse.cs
├── Views/                # Vues Razor
│   ├── Doctors/
│   ├── Patients/
│   ├── Appointments/
│   └── Prescriptions/
├── Data/                 # Contexte et migrations
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs
└── wwwroot/             # Fichiers statiques
```

## 🔐 Autorisations par rôle

| Fonctionnalité | Admin | Doctor | Receptionist |
|----------------|-------|--------|--------------|
| Gérer médecins | ✅ | ❌ | ✅ |
| Gérer patients | ✅ | ✅ | ✅ |
| Gérer rendez-vous | ✅ | ✅ | ✅ |
| Gérer prescriptions | ✅ | ✅ | ❌ |
| Consulter données | ✅ | ✅ | ✅ |

## 📝 Notes

- Le seeder s'exécute automatiquement au démarrage
- Les données ne sont pas recréées si elles existent déjà
- La base de données est créée automatiquement lors de la première migration
- Les identifiants sont également sauvegardés dans `IDENTIFIANTS.txt`

## 🐛 Support

En cas de problème, vérifiez :
1. Que SQL Server est démarré
2. Que la chaîne de connexion dans `appsettings.json` est correcte
3. Que les ports 6000 (HTTP) ou 6001 (HTTPS) ne sont pas utilisés par un autre processus
4. Que toutes les migrations ont été appliquées (`dotnet ef database update`)

