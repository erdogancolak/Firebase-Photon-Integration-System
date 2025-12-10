# ☁️ Unity Online Backend Framework

![Unity](https://img.shields.io/badge/Unity-6000.1.10f1-000000?style=flat&logo=unity)
![Firebase](https://img.shields.io/badge/Firebase-Auth%20%26%20Database-FFCA28?style=flat&logo=firebase)
![Photon](https://img.shields.io/badge/Photon-PUN%202-00B2FF?style=flat&logo=photon)
![Status](https://img.shields.io/badge/Status-Prototype-blue)

> A scalable backend solution for Unity games featuring secure Authentication, Real-time Social Systems, and Multiplayer Networking integration.

---

## 📖 Project Overview

This project serves as a **production-ready backend template** for multiplayer games. It integrates **Firebase** for user management and data persistence, paired with **Photon PUN 2** for real-time networking.

The primary goal is to demonstrate a secure and synchronized **Lobby & Social System** where players can interact before entering gameplay.

---

## 🛠️ Key Features

### 1. Secure Authentication (Firebase Auth)
* **Login & Register:** Secure email/password authentication flow.
* **Error Handling:** Custom error messages for wrong passwords, invalid emails, or connection timeouts.
* **Auto-Login:** Remembers user session for seamless entry.

### 2. Advanced Social System
* **Friend List Management:** Send, accept, or decline friend requests in real-time.
* **Online Status:** Users can see if their friends are "Online", "In-Game", or "Offline" instantly via Firebase Realtime Database.
* **User Search:** Efficient database querying to find players by username.

### 3. Real-Time Chat & Admin Monitoring
* **Global & Private Chat:** Instant messaging system between users.
* **Database Sync:** Unlike ephemeral chat systems, all messages are logged to **Firebase Realtime Database**.
* **Admin Monitoring:** Developers can monitor chat logs directly from the Firebase Console for moderation purposes.

### 4. Multiplayer Integration (Photon PUN 2)
* **Lobby Connection:** Auto-connect to Photon Master Server upon successful login.
* **Room Management:** Create or join game rooms synced with the UI.

---

## ⚙️ Architecture & Tech Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Engine** | Unity 2022+ | Core application logic (C#). |
| **Auth** | Firebase Auth | User identity management. |
| **Database** | Firebase RTDB | Storing player stats, friend lists, and chat logs. |
| **Network** | Photon PUN 2 | Handling low-latency multiplayer packets. |

---

## 📸 System Previews

### Authentication & Lobby UI
*(Screenshots coming soon...)*

### Database Structure (Admin View)
*(Database architecture preview coming soon...)*

---

## 🚀 Future Roadmap

- [ ] Implement "Guild/Clan" system.
- [ ] Add Voice Chat support (Photon Voice).
- [ ] Integrate Leaderboard system using Firebase Cloud Functions.

---

### 👨‍💻 Developer
Developed by **[Erdoğan Çolak](https://www.linkedin.com/in/erdogancolak/)**
