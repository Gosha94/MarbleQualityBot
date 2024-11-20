# cd C:\ngrok

# ngrok http https://localhost:your_port
# Then you'll get Forwarding url like: https://6d93-37-99-26-227.ngrok-free.app

# Set up webhook call from Telegram to your backend api, go to Postman and run:

curl --location 'https://api.telegram.org/bot(YOUR_TELEGRAM_API_TOKEN)/setWebhook' \
--form 'url="(YOUR_EXTERNAL_NGROK_URL)/api/marble-quality-bot-backend/webhook"'

# Then start sending messages into your telegram chat, Done!