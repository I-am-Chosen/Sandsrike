# ─────────────────────────────────────────────────────────────────────────────
# Sandsrike — Unity Dedicated Server
#
# Two modes (detected automatically at container startup):
#
#   PRODUCTION  — ServerBuild/Sandsrike.x86_64 exists (built by GitHub Actions)
#                 Starts the Unity headless game server on $PORT
#
#   PLACEHOLDER — ServerBuild/ is empty (no Unity build yet)
#                 Starts a minimal HTTP server so Render health-check passes
#                 Returns 200 OK on all routes with a status message
#
# Build the Unity project first:
#   File → Build Settings → Dedicated Server (Linux x86_64)
#   Output folder: ServerBuild/
# ─────────────────────────────────────────────────────────────────────────────

FROM ubuntu:22.04

# Unity Linux runtime deps + Python3 for the placeholder health server
RUN apt-get update && apt-get install -y --no-install-recommends \
        ca-certificates \
        libatomic1 \
        libc6 \
        libgcc-s1 \
        libstdc++6 \
        libglib2.0-0 \
        libdbus-1-3 \
        libpthread-stubs0-dev \
        python3 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /server

# Copy whatever is in ServerBuild (may be empty — just .gitkeep — or a real build)
COPY ./ServerBuild/ .

# Make the Unity binary executable if it exists
RUN [ -f "./Sandsrike.x86_64" ] && chmod +x ./Sandsrike.x86_64 || true

# Startup script: runs Unity server if binary is present, else HTTP placeholder
RUN printf '%s\n' \
    '#!/bin/bash' \
    'PORT="${PORT:-7777}"' \
    'if [ -f "/server/Sandsrike.x86_64" ]; then' \
    '  echo "[Sandsrike] Starting Unity dedicated server on port $PORT"' \
    '  exec /server/Sandsrike.x86_64 -batchmode -nographics -logFile -' \
    'else' \
    '  echo "[Sandsrike] Unity build not found — running HTTP placeholder on port $PORT"' \
    '  python3 -c "' \
    'import os, http.server' \
    'port = int(os.environ.get(\"PORT\", 7777))' \
    'class H(http.server.BaseHTTPRequestHandler):' \
    '    def do_GET(self):' \
    '        body = b\"Sandsrike Game Server - awaiting Unity build\"' \
    '        self.send_response(200)' \
    '        self.send_header(\"Content-Length\", len(body))' \
    '        self.end_headers()' \
    '        self.wfile.write(body)' \
    '    def log_message(self, *a): pass' \
    'print(f\"Placeholder listening on {port}\")' \
    'http.server.HTTPServer((\"0.0.0.0\", port), H).serve_forever()' \
    '"' \
    'fi' \
    > /server/start.sh && chmod +x /server/start.sh

EXPOSE 7777

CMD ["/server/start.sh"]
