# Learning Notes

That's a great observation! You've witnessed one of Entity Framework Core's most powerful features in action. Let me explain exactly how EF Core handles this scenario:

## **How Entity Framework Core Handles Column Removal**

### **1. Schema vs Data Separation**
Entity Framework Core treats **schema changes** (structure) separately from **data preservation**:

- **Schema**: The table structure (columns, indexes, constraints)
- **Data**: The actual rows and their values

When you remove a property from your entity class, EF Core only removes the **column definition** from the table schema, but it doesn't delete the **existing rows** - it just removes that specific column from all rows.

### **2. The Migration Process**

Let me show you what actually happened in your migration: Let me check the most recent migration that was created when you removed the City column: Perfect! Now I can show you exactly what happened. Look at this specific line in the migration:

```csharp
migrationBuilder.DropColumn(
    name: "City",
    table: "Users");
```

## **The Technical Process Explained**

### **What EF Core Generated:**
When you removed the `City` property from your `User` entity and created a migration, EF Core generated a `DropColumn` operation. Here's what this SQL command does behind the scenes:

```sql
-- This is what EF Core executes:
ALTER TABLE Users DROP COLUMN City;
```

### **How Database Engines Handle `DROP COLUMN`:**

1. **SQLite (your database):**
   ```sql
   -- SQLite actually recreates the entire table:
   BEGIN TRANSACTION;
   
   -- Create new table without City column
   CREATE TABLE Users_new (
       Id INTEGER PRIMARY KEY,
       Username TEXT,
       PasswordHash TEXT,
       Email TEXT,
       FirstName TEXT,
       LastName TEXT,
       Role TEXT,
       IsActive BOOLEAN,
       CreatedAt TEXT,
       LastLoginAt TEXT
   );
   
   -- Copy all data EXCEPT City column
   INSERT INTO Users_new 
   SELECT Id, Username, PasswordHash, Email, FirstName, LastName, 
          Role, IsActive, CreatedAt, LastLoginAt 
   FROM Users;
   
   -- Drop old table and rename new one
   DROP TABLE Users;
   ALTER TABLE Users_new RENAME TO Users;
   
   COMMIT;
   ```

2. **SQL Server/PostgreSQL:**
   ```sql
   -- These databases can drop columns directly:
   ALTER TABLE Users DROP COLUMN City;
   ```

### **Why Your Data Stayed Intact:**

1. **Row Preservation**: The `DROP COLUMN` operation removes the column definition but **keeps all existing rows**
2. **Selective Column Removal**: Only the `City` column data is discarded; all other columns remain untouched
3. **Primary Key Continuity**: Your `Id` (primary key) values remain the same
4. **Foreign Key Relationships**: All relationships to other tables stay intact

### **Visual Representation:**

**Before Migration:**
```
Users Table:
| Id | Username | Email        | FirstName | LastName | City     | Role | ...
|----|----------|--------------|-----------|----------|----------|------|
| 1  | john123  | john@x.com   | John      | Doe      | New York | User |
| 2  | jane456  | jane@x.com   | Jane      | Smith    | LA       | User |
```

**After Migration:**
```
Users Table:
| Id | Username | Email        | FirstName | LastName | Role | ...
|----|----------|--------------|-----------|----------|------|
| 1  | john123  | john@x.com   | John      | Doe      | User |
| 2  | jane456  | jane@x.com   | Jane      | Smith    | User |
```

### **Key Points:**

- **Data Integrity**: EF Core ensures no data corruption occurs  
- **Atomic Operation**: The entire change happens in a transaction  
- **Rollback Capability**: The `Down()` method can restore the column (but not the data!)  
- **Performance**: Modern databases handle column drops efficiently  

### **Important Warning:**
**Data Loss**: Once you run `dotnet ef database update`, the `City` data is **permanently lost**. Even if you roll back the migration, you'll get the column back but with empty/default values.

This is why EF Core migrations are so powerful - they give you precise control over schema evolution while maintaining data integrity for the columns that remain!