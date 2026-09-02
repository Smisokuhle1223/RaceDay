# RaceDay

RaceDay is a full-stack, web-based event management system built for the South African road running, walking, and cycling community. It replaces the paper-based registration, spreadsheets, and disconnected communication that most local road events still rely on, giving Organisers a single place to manage events and Participants a single place to enter races and track how they've done.

This repository holds the Portfolio of Evidence for RaceDay, submitted progressively across three parts. Part 1 covers the system planning and database work described below.

## System Overview

Organisers use RaceDay to create events, add participation categories to each event (e.g. a 10km fun run and a half marathon under the same event), and capture results once an event has taken place. Participants use RaceDay to create an account, browse upcoming events, enter a category of their choice, and view their own enrolment and results history to track their performance over time.

## Roles

| Role | Permissions |
|---|---|
| **Organiser** | Create, edit, and delete events; manage event categories; capture participant results; view all enrolments for their events. |
| **Participant** | Create an account; browse events; enter an event by selecting a category; view their own enrolments; track their personal results history. |

## Repository Structure

```
/docs
  ERD.png                 - Entity Relationship Diagram for the RaceDay database
  API_Endpoint_Plan.md    - Full endpoint plan (method, route, role, body, response)
  RaceDay_Schema.sql      - SQL Server script: CREATE TABLE + seed data
.github/workflows/        - CI/CD workflow (validates repository structure)
README.md                 - This file
```

## Database

The database has six entities: **Users**, **Venues**, **Events**, **Categories**, **Enrolments**, and **Results**. Users hold both Organisers and Participants, distinguished by a `Role` column. An Organiser creates Events at a Venue; each Event has one or more Categories (e.g. distances); Participants enrol in a Category, and each Enrolment can produce one Result once the event has been run.

See `/docs/ERD.png` for the full diagram and `/docs/RaceDay_Schema.sql` for the script that creates and seeds the schema.

## Setup Instructions

1. **Clone the repository**
   ```
   git clone <this-repo-url>
   cd RaceDay
   ```

2. **Set up the database**
   - Open SQL Server Management Studio (SSMS).
   - Open `/docs/RaceDay_Schema.sql`.
   - Run the script against a fresh SQL Server instance. It creates the `RaceDayDB` database, all six tables with their constraints, and seeds sample data (2 Organisers, 2 Participants, 3 Events, categories per event, and sample enrolments/results).

3. *(Added in Part 2)* API setup instructions - install dependencies, configure the connection string/environment variables, and run the API.

4. *(Added in Part 3)* Front-end setup instructions and any containerisation/deployment steps.

## CI/CD

A GitHub Actions workflow lives in `.github/workflows/` and validates the repository structure for this part (e.g. confirming `/docs` exists and contains the ERD, endpoint plan, and SQL script).

**Successful build screenshot:**

_[Insert screenshot of a green CI/CD build here before submission.]_

## Video Presentation

An unlisted YouTube video walking through the planning documents, ERD decisions, endpoint plan choices, and the SQL script running live in SSMS:

**Video link:** _[Insert unlisted YouTube link here before submission.]_
