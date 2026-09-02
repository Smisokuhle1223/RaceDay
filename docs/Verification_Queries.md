# RaceDay – Verification Queries

After running `RaceDay_Schema.sql` in SSMS, use the queries below to confirm the schema and seed data loaded correctly.

```sql
-- Should return 4 rows: 2 Organisers, 2 Participants
SELECT * FROM dbo.Users;

-- Should return 3 rows
SELECT * FROM dbo.Venues;

-- Should return 3 rows: Comrades Marathon, Cape Town Cycle Tour, Soweto Marathon
SELECT * FROM dbo.Events;

-- Should return 6 rows: 2 categories per event
SELECT * FROM dbo.Categories;

-- Should return 4 rows
SELECT * FROM dbo.Enrolments;

-- Should return 2 rows
SELECT * FROM dbo.Results;
```

## Quick relational check

To confirm the foreign keys are linking correctly, this joins Enrolments back to the Participant's name and the Category they entered:

```sql
SELECT 
    u.FullName AS Participant,
    c.CategoryName,
    e.Status
FROM dbo.Enrolments e
JOIN dbo.Users u ON e.ParticipantID = u.UserID
JOIN dbo.Categories c ON e.CategoryID = c.CategoryID;
```