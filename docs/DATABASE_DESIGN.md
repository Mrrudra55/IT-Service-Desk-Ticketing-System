# Database Design & Schema Documentation

The system uses **SQLite** through **Entity Framework Core 11 Code-First** migrations.

---

## 1. Entity-Relationship Diagram (ERD)

```mermaid
erDiagram
    USERS ||--o{ TICKETS : "creates (1:M)"
    USERS ||--o{ TICKETS : "assigned to (1:M)"
    USERS ||--o{ COMMENTS : "authors (1:M)"
    USERS ||--o{ NOTIFICATIONS : "receives (1:M)"
    USERS ||--o{ TICKET_HISTORIES : "performed by (1:M)"
    USERS ||--o{ ATTACHMENTS : "uploads (1:M)"

    CATEGORIES ||--o{ TICKETS : "classifies (1:M)"

    TICKETS ||--o{ COMMENTS : "contains (1:M)"
    TICKETS ||--o{ TICKET_HISTORIES : "tracks (1:M)"
    TICKETS ||--o{ NOTIFICATIONS : "references (1:M)"
    TICKETS ||--o{ ATTACHMENTS : "has (1:M)"

    USERS {
        int Id PK
        string Name
        string Email UK
        string PasswordHash
        string Role
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }

    CATEGORIES {
        int Id PK
        string Name
        string Description
        bool IsActive
    }

    TICKETS {
        int Id PK
        string TicketNumber UK
        string Title
        string Description
        int CategoryId FK
        string Status
        string Priority
        int CreatedByUserId FK
        int AssignedToUserId FK
        datetime CreatedAt
        datetime UpdatedAt
        datetime DueAt
        datetime ResolvedAt
    }

    COMMENTS {
        int Id PK
        int TicketId FK
        int UserId FK
        string Content
        datetime CreatedAt
    }

    TICKET_HISTORIES {
        int Id PK
        int TicketId FK
        int ChangedByUserId FK
        string Action
        string OldValue
        string NewValue
        datetime CreatedAt
    }

    NOTIFICATIONS {
        int Id PK
        int UserId FK
        int TicketId FK
        string Title
        string Message
        string Type
        bool IsRead
        datetime CreatedAt
    }

    ATTACHMENTS {
        int Id PK
        int TicketId FK
        string FileName
        string StoredFileName
        string FilePath
        string ContentType
        int FileSize
        int UploadedByUserId FK
        datetime CreatedAt
    }
```

---

## 2. Table Specifications & Constraints

### `Users` Table
- `Id`: INTEGER, Primary Key, Autoincrement.
- `Email`: TEXT, Unique Index (`IX_Users_Email`), Case-Insensitive comparison.
- `PasswordHash`: TEXT, Stores salted BCrypt cryptographic hash.
- `Role`: TEXT, Value restricted to `Employee`, `Agent`, or `Admin`.
- `IsActive`: INTEGER (Boolean), Defaults to `1`. Used for soft account deactivation.

### `Categories` Table
- `Id`: INTEGER, Primary Key, Autoincrement.
- `Name`: TEXT, Not Null (e.g. "Hardware", "Software", "Network").
- `Description`: TEXT.
- `IsActive`: INTEGER (Boolean), Defaults to `1`.

### `Tickets` Table
- `Id`: INTEGER, Primary Key, Autoincrement.
- `TicketNumber`: TEXT, Unique Index (`IX_Tickets_TicketNumber`). Sequential format: `TCK-yyyyMM-XXXX`.
- `CategoryId`: INTEGER, Foreign Key to `Categories.Id` with `ON DELETE RESTRICT` (protects tickets if category is referenced).
- `CreatedByUserId`: INTEGER, Foreign Key to `Users.Id` with `ON DELETE RESTRICT` (protects ticket records if employee leaves).
- `AssignedToUserId`: INTEGER, Nullable Foreign Key to `Users.Id` with `ON DELETE SET NULL` (unassigns ticket if technician account is deleted).
- `Status`: TEXT, Values: `Open`, `InProgress`, `OnHold`, `Resolved`, `Closed`.
- `Priority`: TEXT, Values: `Low`, `Medium`, `High`, `Critical`.
- `DueAt`: TEXT (UTC DateTime), Calculated SLA deadline.
- `ResolvedAt`: TEXT (UTC DateTime), Timestamp when marked Resolved.

### `Comments` Table
- `Id`: INTEGER, Primary Key, Autoincrement.
- `TicketId`: INTEGER, Foreign Key with `ON DELETE CASCADE` (deleting ticket purges thread).
- `UserId`: INTEGER, Foreign Key with `ON DELETE RESTRICT`.
- `Content`: TEXT, Up to 4000 characters.

### `TicketHistories` Table
- `Id`: INTEGER, Primary Key, Autoincrement.
- `TicketId`: INTEGER, Foreign Key with `ON DELETE CASCADE`.
- `ChangedByUserId`: INTEGER, Foreign Key with `ON DELETE RESTRICT`.
- `Action`: TEXT (e.g. `TicketCreated`, `Assigned`, `StatusChanged`, `PriorityChanged`, `CommentAdded`, `AttachmentAdded`).
- `OldValue` & `NewValue`: TEXT, Captures delta changes.

### `Notifications` Table
- `Id`: INTEGER, Primary Key, Autoincrement.
- `UserId`: INTEGER, Foreign Key with `ON DELETE CASCADE`.
- `TicketId`: INTEGER, Nullable Foreign Key with `ON DELETE SET NULL`.
- `IsRead`: INTEGER (Boolean), Defaults to `0`.

### `Attachments` Table
- `Id`: INTEGER, Primary Key, Autoincrement.
- `TicketId`: INTEGER, Foreign Key with `ON DELETE CASCADE`.
- `FileName`: TEXT, Original file name.
- `StoredFileName`: TEXT, Randomized GUID file name.
- `FileSize`: INTEGER, Size in bytes.
- `UploadedByUserId`: INTEGER, Foreign Key with `ON DELETE RESTRICT`.

