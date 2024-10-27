
# ngrok http https://localhost:your_port

# go to Postman and run:

curl --location 'https://api.telegram.org/bot(YOUR_TELEGRAM_API_TOKEN)/setWebhook' \
--form 'url="(YOUR_EXTERNAL_NGROK_URL)/api/safe-head-backend/webhook"'