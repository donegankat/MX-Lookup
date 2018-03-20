# MX-Lookup
Looks up an email domain's MX mail server record.
If any of the MX records that are returned match the pattern of a major, commonly used MX service like gmail.com or outlook.com, the output will log a special message with a short, clean indication that this is an MX server we recognize.
These known MX servers and their patterns are stored in appSettings.json.

## Notes:
- This version is working.
- I only addded a basic number of known MX servers to appSettings.json.
- The program currently doesn't do anything except log the results to the console window.
- This was adapted from .NET 4.6.1 code I wrote a year ago that read from/wrote to a SQL Server database.
