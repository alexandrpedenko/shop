# Use the official Microsoft SQL Server image
FROM mcr.microsoft.com/mssql/server:2019-latest

# Expose the SQL Server port
EXPOSE 1433

# Run SQL Server when the container starts
CMD ["/opt/mssql/bin/sqlservr"]