# ── Stage 1: Build Server ─────────────────────────────────────────────────────
FROM node:22-alpine AS builder

RUN apk add --no-cache openssl

WORKDIR /app

COPY Server/package*.json ./
COPY Server/prisma ./prisma/

RUN npm ci

COPY Server/tsconfig.json ./
COPY Server/src ./src/

RUN npx prisma generate
RUN npx tsc --noEmitOnError false || true

# ── Stage 2: Build MockOperator ───────────────────────────────────────────────
FROM node:22-alpine AS mockop-builder

WORKDIR /mockop

COPY MockOperator/package*.json ./
RUN npm ci

COPY MockOperator/tsconfig.json ./
COPY MockOperator/src ./src/

RUN npx tsc --noEmitOnError false || true

# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM node:22-alpine AS runner

RUN npm install -g pm2

WORKDIR /app

COPY Server/package*.json ./
COPY Server/ecosystem.config.cjs ./

RUN npm ci --omit=dev

COPY --from=builder /app/dist ./dist/

WORKDIR /app/mockop
COPY MockOperator/package*.json ./
RUN npm ci --omit=dev
COPY --from=mockop-builder /mockop/dist ./dist/

WORKDIR /app

EXPOSE 2567 3000 4000

CMD ["pm2-runtime", "start", "ecosystem.config.cjs", "--env", "production"]
