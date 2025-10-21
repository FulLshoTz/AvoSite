# Diogo Rodrigues — Live (Render-ready, Docker)

## Deploy no Render (Blueprint)
1. Faz upload deste código para um repositório GitHub (privado ou público).
2. No Render: **New → Blueprint** e seleciona este repo (tem `render.yaml`).
3. O serviço é criado automaticamente. Define as Environment Variables:
   - `YOUTUBE_API_KEY` (obrigatória)
   - `CHANNEL_ID` (ou `CHANNEL_HANDLE`)
4. Abre a URL pública (HTTPS).

## Segurança da key
- Google Cloud → Credentials → a tua key:
  - API restrictions: **YouTube Data API v3**
  - Application restrictions: **IP addresses** → usa os **Outbound IPs** do Render (Service → **Connect → Outbound**).

## Local (Docker)
docker build -t diogo-live .
docker run -it --rm -p 8080:8080 -e YOUTUBE_API_KEY=AIzaSy... -e CHANNEL_ID=UCxxxx diogo-live
# http://localhost:8080