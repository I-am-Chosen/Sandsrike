# ─────────────────────────────────────────────────────────────────────────────
# Sandsrike — Unity Dedicated Server
# Base image: Ubuntu 22.04 (matches Unity Linux player requirements)
#
# Build the Unity project first:
#   File → Build Settings → Platform: Dedicated Server (Linux x86_64)
#   Build Output Folder: ServerBuild/
#
# Then build this image:
#   docker build -t sandsrike-server .
#
# Run locally for testing:
#   docker run -e PORT=7777 -p 7777:7777 sandsrike-server
# ─────────────────────────────────────────────────────────────────────────────

FROM ubuntu:22.04

# Unity Linux dedicated server runtime dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
        ca-certificates \
        libatomic1 \
        libc6 \
        libgcc-s1 \
        libstdc++6 \
        libglib2.0-0 \
        libdbus-1-3 \
        libpthread-stubs0-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /server

# Copy the Unity Dedicated Server build output
COPY ./ServerBuild/ .

# Make the binary executable (Unity names it after the project)
RUN chmod +x ./Sandsrike.x86_64

# Render.com injects PORT at runtime; expose it for documentation purposes
EXPOSE 7777

# -batchmode   — headless, no graphics window
# -nographics  — skip graphics device initialization
# -logFile -   — send logs to stdout (visible in Render.com logs)
# DedicatedServerBootstrap reads PORT env var; -port arg is a fallback
CMD ["./Sandsrike.x86_64", \
     "-batchmode", \
     "-nographics", \
     "-logFile", "-"]
