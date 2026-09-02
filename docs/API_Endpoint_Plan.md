# RaceDay – API Endpoint Plan

> Route naming convention: all resource names are plural and lowercase (e.g. `/api/events`, `/api/categories`), nested resources follow `/api/{parent}/{id}/{child}`, and IDs are always path parameters, never query strings.
Planned before any API code is written, in line with Part 1 Section B. Covers Authentication, User Profile, Events, Categories, Event Enrolments, and Results, matching the ERD in `ERD.png` and the schema in `RaceDay_Schema.sql`.

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Creates a new user account as either an Organiser or a Participant. | None (public) | `{ fullName, email, password, role }` | 201 Created – new user (no password) returned. 400 Bad Request – missing/invalid fields. 409 Conflict – email already registered. |
| POST | /api/auth/login | Authenticates a user and returns a JWT for subsequent requests. | None (public) | `{ email, password }` | 200 OK – `{ token, user }`. 401 Unauthorized – invalid credentials. |

## User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/profile | Returns the logged-in user's own profile details. | Any (logged in) | None | 200 OK – user profile. 401 Unauthorized. |
| PUT | /api/users/profile | Updates the logged-in user's own name/email. | Any (logged in) | `{ fullName, email }` | 200 OK – updated profile. 400 Bad Request. 401 Unauthorized. |
| GET | /api/users/{id} | Returns a specific user's public profile (e.g. an organiser's name for an event listing). | Any (logged in) | None | 200 OK – public profile fields. 404 Not Found. |

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events | Creates a new event owned by the logged-in organiser. | Organiser | `{ eventName, description, eventDate, venueId }` | 201 Created – new event. 400 Bad Request. 403 Forbidden – not an organiser. |
| GET | /api/events | Lists all upcoming events for participants to browse. | None (public) | None | 200 OK – array of events. |
| GET | /api/events/{id} | Returns full details of a single event, including its categories. | None (public) | None | 200 OK – event with categories. 404 Not Found. |
| PUT | /api/events/{id} | Updates an event's details. | Organiser (owner) | `{ eventName, description, eventDate, venueId }` | 200 OK – updated event. 403 Forbidden – not the owning organiser. 404 Not Found. |
| DELETE | /api/events/{id} | Deletes an event and its related categories. | Organiser (owner) | None | 200 OK. 403 Forbidden. 404 Not Found. |

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events/{id}/categories | Adds a new participation category (e.g. 5km, 10km) to an event. | Organiser (owner) | `{ categoryName, distanceKm, maxParticipants, entryFee }` | 201 Created – new category. 400 Bad Request. 403 Forbidden. 404 Not Found – event does not exist. |
| GET | /api/events/{id}/categories | Lists all categories available for a specific event. | None (public) | None | 200 OK – array of categories. 404 Not Found. |
| PUT | /api/categories/{id} | Updates an existing category's details. | Organiser (owner) | `{ categoryName, distanceKm, maxParticipants, entryFee }` | 200 OK – updated category. 403 Forbidden. 404 Not Found. |
| DELETE | /api/categories/{id} | Removes a category from an event. | Organiser (owner) | None | 200 OK. 403 Forbidden. 404 Not Found. |

## Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/categories/{id}/enrolments | Enrols the currently logged-in participant into the specified category, provided space is still available. | Participant | None | 201 Created – enrolment record. 400 Bad Request – category full. 404 Not Found. 409 Conflict – already enrolled. |
| GET | /api/enrolments/my | Returns the logged-in participant's own enrolment history. | Participant | None | 200 OK – array of the participant's enrolments. |
| GET | /api/events/{id}/enrolments | Returns all enrolments for an event, for the organiser to manage. | Organiser (owner) | None | 200 OK – array of enrolments. 403 Forbidden. 404 Not Found. |
| DELETE | /api/enrolments/{id} | Cancels the logged-in participant's own enrolment. | Participant (owner) | None | 200 OK. 403 Forbidden. 404 Not Found. |

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments/{id}/results | Captures a race result against a participant's enrolment. | Organiser | `{ finishTime, position, status }` | 201 Created – new result. 400 Bad Request. 403 Forbidden. 404 Not Found. 409 Conflict – result already captured. |
| GET | /api/enrolments/{id}/results | Returns the captured result for a specific enrolment, visible only to the participant who owns it or the organiser of the event. | Any (owning participant or event organiser) | None | 200 OK – result. 403 Forbidden. 404 Not Found – no result yet. | |
| GET | /api/users/{id}/results | Returns a participant's complete performance history — every result they've received across all events they've entered. | Participant (owner) or Organiser | None | 200 OK – array of results. 403 Forbidden. | |
| PUT | /api/results/{id} | Corrects/updates an already captured result. | Organiser | `{ finishTime, position, status }` | 200 OK – updated result. 403 Forbidden. 404 Not Found. |
