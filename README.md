# Soil Contamination API

A minimal CRUD API built with .NET 9.0 to manage soil contamination guidelines and analyze compliance with measured values. This project uses LiteDB for data storage and provides endpoints for managing guidelines and analyzing soil contamination data.

---

## Features

- **CRUD Operations**: Create, Read, Update, and Delete guidelines for soil contamination.
- **Analysis Endpoint**: Analyze compliance of measured values against guidelines.
- **Seed Data**: Preload guidelines from a JSON file.

---

## Endpoints

### `/manage`
- **GET** `/manage`: Retrieve all guidelines.
- **POST** `/manage`: Add a new guideline.
- **PUT** `/manage/{id}`: Update an existing guideline.
- **DELETE** `/manage/{id}`: Delete a guideline.

### `/analyze`
- **POST** `/analyze`: Analyze compliance of measured values against guidelines.
