# ─────────────────────────────────────────────────────────────
# Stage 1: Build & Publish
# ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer-cached restore
COPY backend/src/ProposalManagement.Domain/ProposalManagement.Domain.csproj \
     backend/src/ProposalManagement.Domain/
COPY backend/src/ProposalManagement.Application/ProposalManagement.Application.csproj \
     backend/src/ProposalManagement.Application/
COPY backend/src/ProposalManagement.Infrastructure/ProposalManagement.Infrastructure.csproj \
     backend/src/ProposalManagement.Infrastructure/
COPY backend/src/ProposalManagement.Api/ProposalManagement.Api.csproj \
     backend/src/ProposalManagement.Api/

# Restore dependencies (API project pulls in all transitive deps)
RUN dotnet restore backend/src/ProposalManagement.Api/ProposalManagement.Api.csproj

# Copy full source and publish
COPY backend/src/ backend/src/

RUN dotnet publish backend/src/ProposalManagement.Api/ProposalManagement.Api.csproj \
        -c Release \
        --no-restore \
        -o /app/publish

# ─────────────────────────────────────────────────────────────
# Stage 2: Runtime
# ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# libgdiplus  – PdfSharp GDI+
# libfontconfig1 / libfreetype6 – font enumeration (QuestPDF + PdfSharp)
# fonts-liberation – metric-compatible Arial/Times/Courier substitutes
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        libgdiplus \
        libfontconfig1 \
        libfreetype6 \
        fonts-liberation \
 && fc-cache -f \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Pre-create writable directories for file storage and signature images
RUN mkdir -p uploads wwwroot/images/signatures \
 && chmod -R 755 uploads wwwroot/images/signatures

# Declare as volumes so the host / orchestrator can mount persistent storage
VOLUME ["/app/uploads", "/app/wwwroot/images/signatures"]

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ProposalManagement.Api.dll"]
