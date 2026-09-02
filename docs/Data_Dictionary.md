# RaceDay – Data Dictionary

Full column listing for all six tables in the RaceDay database, matching `ERD.png` and `RaceDay_Schema.sql`.

## Users

| Column | Type | Description |
|---|---|---|
| UserID | INT, PK, Identity | Unique identifier for the user. |
| FullName | VARCHAR(100) | The user's full name. |
| Email | VARCHAR(150), Unique | Login email, must be unique across all users. |
| PasswordHash | VARCHAR(255) | Encrypted/hashed password (never stored in plain text). |
| Role | VARCHAR(20) | Either 'Organiser' or 'Participant'. |
| CreatedAt | DATETIME | When the account was created, defaults to the current date/time. |

## Venues

| Column | Type | Description |
|---|---|---|
| VenueID | INT, PK, Identity | Unique identifier for the venue. |
| VenueName | VARCHAR(150) | Name of the venue/stadium/starting point. |
| City | VARCHAR(100) | City the venue is located in. |
| Province | VARCHAR(100) | Province the venue is located in. |
| Latitude | DECIMAL(9,6) | GPS latitude, used for live weather/route lookups. |
| Longitude | DECIMAL(9,6) | GPS longitude, used for live weather/route lookups. |

## Events

| Column | Type | Description |
|---|---|---|
| EventID | INT, PK, Identity | Unique identifier for the event. |
| EventName | VARCHAR(150) | Name of the event. |
| Description | VARCHAR(500) | Short description of the event. |
| EventDate | DATETIME | Date and time the event takes place. |
| VenueID | INT, FK → Venues | The venue hosting this event. |
| OrganiserID | INT, FK → Users | The organiser who created this event. |
| CreatedAt | DATETIME | When the event record was created. |

## Categories

| Column | Type | Description |
|---|---|---|
| CategoryID | INT, PK, Identity | Unique identifier for the category. |
| EventID | INT, FK → Events | The event this category belongs to. |
| CategoryName | VARCHAR(100) | Name of the category/distance (e.g. "10km Fun Run"). |
| DistanceKM | DECIMAL(6,2) | Distance of this category in kilometres. |
| MaxParticipants | INT | Maximum number of participants allowed to enrol. |
| EntryFee | DECIMAL(8,2) | Cost to enter this category, in Rands. |

## Enrolments

| Column | Type | Description |
|---|---|---|
| EnrolmentID | INT, PK, Identity | Unique identifier for the enrolment. |
| ParticipantID | INT, FK → Users | The participant who enrolled. |
| CategoryID | INT, FK → Categories | The category they enrolled in. |
| EnrolmentDate | DATETIME | When the enrolment was made. |
| Status | VARCHAR(20) | 'Pending', 'Confirmed', or 'Cancelled'. |

## Results

| Column | Type | Description |
|---|---|---|
| ResultID | INT, PK, Identity | Unique identifier for the result. |
| EnrolmentID | INT, FK → Enrolments, Unique | The enrolment this result belongs to (one result per enrolment). |
| FinishTime | TIME | Time taken to finish, if applicable. |
| Position | INT | Finishing position, if applicable. |
| Status | VARCHAR(20) | 'Finished', 'DNF' (Did Not Finish), or 'DNS' (Did Not Start). |
| RecordedAt | DATETIME | When the result was captured. |