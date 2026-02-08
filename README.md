# Visualizing Environmental Data in Digital Twin

## Purpose & Goals

The main goal of this project is to read environmental data from a database and visualize this data in a Digital Twin environment using Unity.

## Tasks in the Project

1.  Innovate different ways to visualize environmental data in Digital Twins.
2.  Investigate how environmental data should be stored effectively.
3.  Test how external data can be read into Unity.
4.  Create data visualizations in Unity.
5.  Add data visualizations to the City of Helsinki's digital twin.

## Operating Mode

This is a fully agile project, and requirements may be updated during the project lifecycle.

## Members

- Jia Ke
- Ma Jing
- Pan Tingyu

## Architecture

### Backend

The backend is built using the **Django** framework and **Django Rest Framework (DRF)** to provide a RESTful API for the frontend.

- **Project Structure**: The main project configuration is located in the `config` directory.
- **Dependencies**: Python packages are managed with `pip` and are listed in the `requirements.txt` file. It is strongly recommended to use the provided `.venv` virtual environment.

### Frontend

The frontend is a **Unity** application. It acts as the client that consumes data from the backend API to render visualizations within the digital twin environment.

## Backend Setup

1.  Navigate to the `backend` directory:
    ```sh
    cd backend
    ```

2.  Create a Python virtual environment:
    ```sh
    python3 -m venv .venv
    ```

3.  Activate the virtual environment:
    - On macOS/Linux:
      ```sh
      source .venv/bin/activate
      ```
    - On Windows:
      ```sh
      .venv\Scripts\activate
      ```

4.  Install the required packages:
    ```sh
    pip install -r requirements.txt
    ```

5.  Run the development server:
    ```sh
    python manage.py runserver
    ```
