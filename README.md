# ShiftDesk

**CSC 2210 - Object Oriented Programming 2 (C#) - Lab 1 (Resubmission)**
Student ID: 21-45717-3

A Windows Forms application with a sign-in screen, a registration screen and a
dashboard behind them. User accounts are kept in **Microsoft SQL Server**, and
the connection string lives in `App.config`.

The lab started from a version of this program that kept its users in a
Microsoft Access `.mdb` file through `System.Data.OleDb`. Moving it onto SQL
Server was the task. None of the Access code survives.

---

## What the application does

**Sign in.** Type a username and a password. The application hashes the
password and asks SQL Server how many rows of `tbl_users` carry that username
and that hash. One row means the dashboard opens. Anything else gets a refusal
and the password box is emptied. A *Show password* tick reveals what has been
typed, and there are *Clear* and *Exit* buttons.

**Register.** Type a username, a password, and the password again. Three things
are checked before anything is written: no field is empty, the two passwords
agree, and the username is not already in the table. Only then is a row
inserted.

**Dashboard.** The screen behind the sign-in. It names whoever signed in and
has a *Log out* button, which asks for confirmation and then returns to the
sign-in screen with the boxes cleared. It does not shut the program down -
that is the difference between logging out and quitting.

---

## How to run it

You need **SQL Server** (any edition - LocalDB, Express or full) and **Visual
Studio** with **.NET Framework 4.7.2**.

**1. Create the database.** Open `database.sql` and run the whole file. It
creates `db_users`, creates `tbl_users`, and adds one test account. Running it
twice is safe - every step checks first, so it will not wipe accounts
registered during testing.

**2. Point the application at your server.** Open `ShiftDesk/App.config`. It
ships set to LocalDB. Change **only** the `Data Source=` part if your setup
differs:

| Your SQL Server | `Data Source=` |
| --- | --- |
| LocalDB (installs with Visual Studio) | `(localdb)\MSSQLLocalDB` |
| Default instance on this machine | `.` or `localhost` |
| SQL Server Express | `.\SQLEXPRESS` |
| Express on a named machine | `DESKTOP-ABC123\SQLEXPRESS` |

Leave the rest of the line alone.

**3. Run.** Open `ShiftDesk.sln` and press F5. The sign-in screen opens first.

**4. Test account.**

```
username: admin
password: admin123
```

If you get *"Could not reach SQL Server"*, the `Data Source=` in `App.config`
does not match your machine, or `database.sql` has not been run yet.

---

## What I changed, and why

### Getting rid of Access

The starting version opened its database like this:

```csharp
OleDbConnection con = new OleDbConnection(
    "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = db_users.mdb");
```

Two problems with that. The Jet provider is a separate piece of software that
has to already be installed, and on a 64-bit machine that has never had Office
on it, it simply is not there - so the program throws before it does anything.
The bigger problem is that an `.mdb` file is not a database server. It is a
file sitting next to the executable. There is no login on it, no permissions,
nothing stopping anyone copying the whole thing onto a USB stick.

Everything now goes through `System.Data.SqlClient` instead. `SqlConnection`
opens the connection, `SqlCommand` carries the statement and its parameters.
`ExecuteScalar()` is used where the answer is a single value - both the sign-in
check and the duplicate-username check are `SELECT COUNT(*)`, which returns one
number. `ExecuteNonQuery()` is used for the `INSERT`, because an insert hands
back a row count rather than a result set.

Every connection is opened inside a `using` block, so it closes and goes back
to the connection pool even if something throws halfway through.

### Files I wrote or edited

| File | What it is |
| --- | --- |
| `Data/UserStore.cs` | **New.** Every SQL statement in the program. Reads the connection string, runs the three queries. |
| `Security/PasswordHasher.cs` | **New.** Turns a password into a SHA-256 digest. |
| `UI/Theme.cs` | **New.** The palette, and the focus indicator that cannot be expressed in the designer. |
| `frmLogin.cs` | Rewritten. Hands the username and password to `UserStore`, opens the dashboard on success. |
| `frmRegister.cs` | Rewritten. Three validation checks, then `UserStore.CreateUser`. |
| `frmDashboard.cs` | Logout now returns to the sign-in screen instead of ending the program. |
| `Program.cs` | Starts on `frmLogin`. The original started on the registration form. |
| `App.config` | Holds the `connString` connection string. |
| `database.sql` | **New.** Creates the database, the table and the test account. |

The project also needs the **System.Configuration** assembly reference, which
is in `ShiftDesk.csproj`. Without it nothing compiles, and the error -
*"ConfigurationManager does not exist in the current context"* - does not point
at a missing reference in any obvious way.

### Why the connection string is in App.config

The lazy version of this is to paste the connection string at the top of
`frmLogin` and again at the top of `frmRegister`. I did not do that, and the
reason is not tidiness.

The server name is the one thing in this project guaranteed to be wrong on
somebody else's computer. Mine is not the marker's, and the marker's is not the
lab machine's. If the string is written into the code, changing it means
editing source and rebuilding. In `App.config` it is a one-line edit in a text
file next to the `.exe`, and the program picks it up next time it starts.

Two copies is also two things to forget. Update the one in `frmLogin` and miss
the one in `frmRegister`, and you get an application where signing in works and
registering does not, with nothing on screen to explain why.

So there is exactly one copy, and it is read in exactly one place -
`Data/UserStore.cs`:

```csharp
private static readonly string ConnectionString =
    ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
```

`ConfigurationManager` is the class that reads `App.config`.
`ConnectionStrings["connString"]` picks the entry whose `name` is `connString`,
and `.ConnectionString` pulls the text out of it. It is `static readonly`, so
it is read once when the class is first touched rather than on every click.

Neither form contains a connection string, a `SqlConnection`, or a line of SQL.

### What @username and @password are for

The original built its query by gluing strings together:

```csharp
string login = "SELECT * FROM tbl_users WHERE username = '" + txtUsername.Text +
               "' and password = '" + txtPassword.Text + "'";
```

Whatever the user types becomes part of the command itself. Put `' OR '1'='1`
in the password box and SQL Server receives:

```sql
SELECT * FROM tbl_users WHERE username = '' and password = '' OR '1'='1'
```

`'1'='1'` is true for every row, so the `WHERE` matches the whole table and you
are signed in as somebody without knowing a password. That is SQL injection.

Mine never assembles a query out of user input:

```csharp
const string sql = "SELECT COUNT(*) FROM tbl_users " +
                   "WHERE username = @username AND password = @password";

command.Parameters.AddWithValue("@username", username);
command.Parameters.AddWithValue("@password", PasswordHasher.Hash(password));
```

`@username` and `@password` are placeholders. The command text is finished
before the user types anything, and the typed values travel to SQL Server
separately, as data. SQL Server slots them in as values and never reads them as
SQL. Typing `' OR '1'='1` looks for an account whose password is literally the
characters `' OR '1'='1`, finds none, and is refused.

---

## Bonus: SHA-256 password hashing

`tbl_users.password` does not hold passwords. It holds the SHA-256 digest of
each one, as 64 hexadecimal characters.

Plain text is unsafe for a reason that has nothing to do with being hacked:
anyone who can read the table can read every password. That is whoever
administers the server, anyone holding a backup file, and anyone who finds an
old copy of the database on a laptop. Because people reuse passwords, a leak
out of a student project can hand somebody the key to that person's email.

A hash only goes one way. The same input always gives the same 64 characters,
and there is no operation that turns those characters back into the password.
The application never needs to know the password - only whether the hash of
what was typed equals the hash that is stored.

**Both sides have to hash, and the way I structured it they cannot not.**
`PasswordHasher.Hash` is called inside `UserStore.CredentialsAreValid` and
inside `UserStore.CreateUser`, and nothing else in the program calls it. The
forms never see a hash. If hashing only happened on registration, every new
account would be created successfully and then be unable to sign in, because
sign-in would be comparing a typed password against a stored digest - and that
is a bug you only notice after you have already recorded your video.

The `admin` row in `database.sql` therefore stores the digest, not the word.
The second query in that file proves they are the same value by having SQL
Server compute `HASHBYTES('SHA2_256', 'admin123')` itself and printing it
beside the stored column.

There is also a `CHECK (LEN(password) = 64)` constraint on the column. A
SHA-256 digest in hex is always 64 characters, so anything shorter is a
plain-text password being written by mistake, and SQL Server rejects it.

If this were real I would add a random per-user salt as well, so two people who
pick the same password do not end up with the same stored digest. That needs a
fourth column, which this lab's table does not have, so plain SHA-256 is where
I stopped.

---

## How the code is organised

```
ShiftDesk/
  Data/UserStore.cs           all SQL, and the only read of App.config
  Security/PasswordHasher.cs  SHA-256
  UI/Theme.cs                 palette + focus indicator
  frmLogin.cs                 sign-in screen
  frmRegister.cs              registration screen
  frmDashboard.cs             behind the sign-in, with Log out
  Program.cs                  starts on frmLogin
  App.config                  the connection string
database.sql                  creates db_users, tbl_users, admin account
```

The handout's version keeps the connection string and the SQL inside each form.
I pulled both into `UserStore` instead. It means the connection string is read
once rather than twice, all the SQL is in one file to look at, and - the part I
actually care about - hashing is not something a form can forget to do, because
no form is in a position to do it at all.

## One thing I did differently

The obvious logout is `new frmLogin().Show(); this.Close();`. I did not use it.

`Program.cs` runs the application on the *first* login form:

```csharp
Application.Run(new frmLogin());
```

That form is hidden, not closed, while the dashboard is up. Creating a second
one on logout would leave the first alive and invisible forever, and because
`Application.Run` is holding it, the process would keep running after the
visible window was closed - the program disappears from the screen but stays in
Task Manager. So `frmLogin` hands the dashboard a reference to itself, and
logout brings that same window back:

```csharp
_loginWindow.ResetForNextUser();
_loginWindow.Show();
Close();
```

Same thing on screen, one login window instead of two, and the program exits
properly when that window is finally closed.

---

## Design

The interface is deliberately a dark console rather than a light form: deep
slate (`#111827`), a single amber accent (`#F59E0B`), Consolas for labels and
the wordmark, Segoe UI for everything else. Flat square controls, no gradients.

Two things in it are not decoration:

- **Focus is visible.** Each field sits on a flat surface with a rule
  underneath, and that rule turns amber while the field has focus. A borderless
  dark input with no focus indicator is unusable with a keyboard.
- **Contrast was measured, not eyeballed.** Every text colour was checked
  against the surface behind it and clears the WCAG AA minimum of 4.5:1 for
  body text. The first pass did not - the hint text came out at 3.7:1 and the
  footer at 2.3:1, and both were lightened until they passed.

---

## Testing

| # | Test | Result |
| --- | --- | --- |
| 1 | Sign in as `admin` / `admin123` | Dashboard opens |
| 2 | Sign in with a wrong password | Refused, no crash, password box cleared |
| 3 | Register a new username | Confirmation message |
| 4 | Sign in with the account just registered | Dashboard opens |
| 5 | Log out, confirm Yes | Back on the sign-in screen, program still running |
| 6 | `SELECT * FROM tbl_users;` | The new account is in the table |
| 7 | Register the same username twice | Second attempt refused |
| 8 | Register with two different passwords | Refused before touching the database |
| 9 | `' OR '1'='1` as the password | Refused - it is searched for as a password |
