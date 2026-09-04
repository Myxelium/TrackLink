FROM mcr.microsoft.com/devcontainers/javascript-node:22-bookworm

# Node 22 LTS matches Angular 19.2.x (^18.19.1 || ^20.11.1 || ^22.0.0).
# Git is already in the base image.

RUN apt-get update \
    && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
        ca-certificates \
        chromium \
        curl \
        fonts-liberation \
        wget \
    && mkdir -p -m 755 /etc/apt/keyrings \
    && wget -nv -O /etc/apt/keyrings/githubcli-archive-keyring.gpg \
        https://cli.github.com/packages/githubcli-archive-keyring.gpg \
    && chmod go+r /etc/apt/keyrings/githubcli-archive-keyring.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" \
        > /etc/apt/sources.list.d/github-cli.list \
    && apt-get update \
    && apt-get -y install --no-install-recommends gh \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

COPY chrome-no-sandbox /usr/local/bin/chrome-no-sandbox
RUN chmod 0755 /usr/local/bin/chrome-no-sandbox

ENV CHROME_BIN=/usr/local/bin/chrome-no-sandbox \
    NG_CLI_ANALYTICS=false \
    LANG=C.UTF-8

# Global CLI is optional; the app also provides @angular/cli via npx after npm install.
RUN su node -c "npm install -g @angular/cli@19"
