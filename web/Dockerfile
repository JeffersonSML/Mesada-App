# syntax=docker/dockerfile:1
# Build context esperado: web/ (na Railway, Root Directory = web).

FROM oven/bun:1 AS build
WORKDIR /app

COPY package.json bun.lock ./
RUN bun install --frozen-lockfile

COPY . .

# O Vite embute VITE_* no bundle em tempo de build, então a URL da API
# precisa existir AQUI (na Railway: variável VITE_API_BASE_URL do serviço).
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN bun run build

FROM node:22-slim AS final
WORKDIR /app
ENV NODE_ENV=production HOST=0.0.0.0 PORT=3000
COPY --from=build --chown=node:node /app/.output ./.output
USER node
EXPOSE 3000
CMD ["node", ".output/server/index.mjs"]
