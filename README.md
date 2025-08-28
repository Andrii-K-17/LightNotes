# LightNotes

**LightNotes** is a lightweight and modular note-taking application designed for simplicity and efficiency.

---

## Features

* **Clean, intuitive interface:** A minimalist design that eliminates visual clutter and helps you focus completely on creativity.
* **Collaborative notes:** A powerful tool for teamwork. Share notes with friends or colleagues to work on ideas together.
* **In-note chat:** Discuss details in real time right inside the note. This makes teamwork more efficient by eliminating the need to switch between different apps.
* **Modular Architecture:** Separates the frontend and backend for scalable development.
* **Lightweight Design:** Focuses on core note-taking functionality without bloat.
* **RESTful API:** Provides a clean and well-defined interface for interaction.

---

## Tech stack

* **Frontend:** Vue.js + TypeScript
* **Backend:** C# / ASP.NET Core Web API
* **Database:** MySQL

### Run the backend and Database (with Docker)

**1. Build and start the backend and database containers in detached mode:**
```bash
cd backend
docker compose up -d --build
```
**2. Verify the services are running:**
```bash
docker compose ps
```

### Run the frontend (with npm)

**1. Install the required npm packages:**
```bash
cd frontend
npm install
```
**2. Start the frontend development server:**
```bash
npm run dev
```
