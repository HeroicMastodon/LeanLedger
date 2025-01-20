# Step 1: Build the frontend
FROM node:20.18 AS build-frontend

# Set the working directory for the frontend build process
WORKDIR /app

# Copy the frontend project files into the container
COPY LeanLedgerApp ./LeanLedgerApp
WORKDIR /app/LeanLedgerApp

RUN npm install

RUN npm run build  # Assuming the build script places output in LeanLedgerServer/StaticFiles

# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy the .NET project files
COPY ./LeanLedgerServer/*.csproj ./
RUN dotnet restore

# Copy the entire backend project and the frontend
COPY . ./

# Build the frontend application
#WORKDIR /app/LeanLedgerApp
#RUN npm install
#RUN npm run build

# Move back to the backend project directory
WORKDIR /app/LeanLedgerServer

# Build the .NET application in Release mode
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet publish -c Release -o /app/out

# Use a runtime image to host the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=build /app/out ./

# Copy the built frontend files from the frontend build stage
COPY --from=build-frontend /app/LeanLedgerServer/StaticFiles /app/StaticFiles

# Expose the port your application uses
EXPOSE 5000

# Entry point for the application
ENTRYPOINT ["dotnet", "LeanLedgerServer.dll"]



