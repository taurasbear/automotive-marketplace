---
name: openai-api-integration
description: Use when making requests to OpenAI API — text completions, chat, embeddings, or any endpoint at https://api.openai.com/v1
---

# OpenAI API Integration

## Overview

OpenAI API provides AI-powered endpoints for chat completions, text generation, embeddings, and other models. Authentication uses Bearer token from `OPENAI_APIKEY` environment variable in the `Authorization` header.

## Quick Reference

| Endpoint | Purpose | Method | Key Details |
|----------|---------|--------|------------|
| `/v1/chat/completions` | Multi-turn conversations | POST | Messages array, role-based (system/user/assistant) |
| `/v1/completions` | Legacy text completion | POST | Deprecated, use chat completions |
| `/v1/embeddings` | Vector embeddings | POST | Convert text to embeddings for similarity |
| `/v1/images/generations` | Generate images | POST | DALL-E model, returns image URLs |
| `/v1/audio/transcriptions` | Speech-to-text | POST | Upload audio file, returns transcript |
| `/v1/audio/speech` | Text-to-speech | POST | Convert text to audio file |

## Authentication

Always set the `Authorization` header with Bearer token. Get API key from environment variable:

```bash
# Load from shell
source ~/.profile  # Sets OPENAI_APIKEY

# In code (Node.js example)
const apiKey = process.env.OPENAI_APIKEY;
const headers = {
  'Authorization': `Bearer ${apiKey}`,
  'Content-Type': 'application/json'
};
```

## Base URL

**Always use:** `https://api.openai.com/v1`

This is the standard OpenAI API endpoint.

## Implementation

### Chat Completions (Primary Endpoint)

**Endpoint:** `POST /v1/chat/completions`

**Use `gpt-5.4-mini` for text-to-text input and output.**

```bash
curl "https://api.openai.com/v1/chat/completions" \
  -H "Authorization: Bearer $OPENAI_APIKEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "gpt-5.4-mini",
    "messages": [
      {
        "role": "system",
        "content": "You are a helpful assistant."
      },
      {
        "role": "user",
        "content": "Write a short bedtime story about a unicorn."
      }
    ],
    "temperature": 0.7,
    "max_tokens": 500
  }'
```

**Request body fields:**
- `model`: Use `"gpt-5.4-mini"` for text-to-text completions
- `messages`: Array of message objects with `role` (system/user/assistant) and `content`
- `temperature`: 0-2, controls randomness (0=deterministic, 2=maximum randomness, default 1)
- `max_tokens`: Maximum response length
- `top_p`: Nucleus sampling (default 1)

**Response:**
```json
{
  "id": "chatcmpl-...",
  "object": "chat.completion",
  "created": 1234567890,
  "model": "gpt-4",
  "usage": {
    "prompt_tokens": 20,
    "completion_tokens": 150,
    "total_tokens": 170
  },
  "choices": [
    {
      "message": {
        "role": "assistant",
        "content": "Once upon a time, in a magical forest..."
      },
      "finish_reason": "stop",
      "index": 0
    }
  ]
}
```

### Embeddings (Vector Representations)

**Endpoint:** `POST /v1/embeddings`

```bash
curl "https://api.openai.com/v1/embeddings" \
  -H "Authorization: Bearer $OPENAI_APIKEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "text-embedding-3-small",
    "input": "The quick brown fox jumps over the lazy dog"
  }'
```

**Response:**
```json
{
  "object": "list",
  "data": [
    {
      "object": "embedding",
      "index": 0,
      "embedding": [0.123, -0.456, 0.789, ...]
    }
  ],
  "model": "text-embedding-3-small",
  "usage": {
    "prompt_tokens": 10,
    "total_tokens": 10
  }
}
```

## Common Mistakes

| Mistake | Fix |
|--------|-----|
| Using `Bearer` instead of `Authorization: Bearer` | Correct format: `-H "Authorization: Bearer $OPENAI_APIKEY"` |
| Using `/responses` endpoint | Use `/chat/completions` or `/completions`, not `/responses` |
| Using `"input"` field instead of `"messages"` | Chat API uses `"messages": [{ "role": "user", "content": "..." }]` |
| Not using `gpt-5.4-mini` for text-to-text | Use `gpt-5.4-mini` for text-to-text input/output; use `gpt-realtime-mini` for image-to-text |
| Forgetting `"role"` in messages | Each message must have `"role": "system"`, `"user"`, or `"assistant"` |
| Sending API key in body/query | Send ONLY in `Authorization` header |
| Not checking for errors | Check for `"error"` field in response |

## Response Format

**Successful response:** Contains `"choices"` array (chat) or `"data"` array (embeddings)

**Error response:**
```json
{
  "error": {
    "message": "Incorrect API key provided",
    "type": "invalid_request_error",
    "param": null,
    "code": "invalid_api_key"
  }
}
```

## Red Flags - STOP and Verify

- Misspelling endpoint as `/responses` instead of `/chat/completions`
- Using custom request body format instead of `"messages"` array
- Using non-existent model names
- Forgetting to `source ~/.profile` to load environment variables
- Not including Bearer token format in Authorization header
- Using GET instead of POST (most endpoints are POST)
- Checking response without first verifying no error field exists

## Available Models

**Text-to-Text Completions:**
- `gpt-5.4-mini` — Use for text-to-text input/output (Recommended for most use cases)

**Image-to-Text:**
- `gpt-realtime-mini` — Use for image-to-text input/output

**Legacy/Deprecated:**
- `gpt-4` — Legacy, don't use
- `gpt-4-turbo` — Legacy, don't use
- `gpt-3.5-turbo` — Legacy, don't use

**Embeddings:**
- `text-embedding-3-small` — Fast, efficient
- `text-embedding-3-large` — More accurate, slower
- `text-embedding-ada-002` — Legacy, don't use

## Important: Streaming

For long responses, use `"stream": true` to get incremental responses:

```bash
curl "https://api.openai.com/v1/chat/completions" \
  -H "Authorization: Bearer $OPENAI_APIKEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "gpt-5.4-mini",
    "messages": [...],
    "stream": true
  }'
```

Responses arrive as `data: {...}` lines (Server-Sent Events format).

## Image-to-Text Input/Output

For image-to-text capabilities, use `gpt-realtime-mini` model. Include images in messages using vision format.

**Example with image:**
```bash
curl "https://api.openai.com/v1/chat/completions" \
  -H "Authorization: Bearer $OPENAI_APIKEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "gpt-realtime-mini",
    "messages": [
      {
        "role": "user",
        "content": [
          {
            "type": "text",
            "text": "What is in this image?"
          },
          {
            "type": "image_url",
            "image_url": {
              "url": "https://example.com/image.jpg"
            }
          }
        ]
      }
    ]
  }'
```

## Documentation

For comprehensive endpoint reference and all parameters, see [OpenAI API Documentation](https://platform.openai.com/docs/api-reference).
