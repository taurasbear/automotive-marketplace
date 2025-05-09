# Development stage
FROM node:22-slim AS development

WORKDIR /app

COPY package*.json ./

RUN npm install

COPY . .

EXPOSE 3000

CMD ["npm", "run", "dev"]

# Production stage
FROM node:22-slim AS build

WORKDIR /app

COPY package*.json ./

RUN npm install

COPY . .

ARG VITE_APP_API_URL
ENV VITE_APP_API_URL=$VITE_APP_API_URL

RUN npm run build

FROM node:22-slim AS production

WORKDIR /app
COPY --from=build /app/dist ./dist

RUN npm install -g serve

EXPOSE 57263

CMD ["serve", "-s", "dist", "-l", "57263"]