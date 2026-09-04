# Band invites

> **Area:** invites
> **Status:** Active
> **Last updated:** 2026-09-04

## Overview

A band owner invites someone by email. TrackLink stores a code and can send mail when SMTP is configured. Anyone can open `/join` and paste email + code (or follow the invite link). Google login must match the invite email before membership is created.

## Responsibilities

- Create invite + optional email
- Accept by email and code
- Does **not** grant Drive ACL; the owner still shares the band folder in Google

## Key concepts

- **Invite**: band + email + code + role (`uploader` or `member`)
- **Accept URL**: `{AppReturnUrl}/join?code=&email=`

## API Endpoint

### Create

- **Method:** POST
- **Path:** `/api/bands/{bandId}/invites`
- **Authentication:** Session cookie; owner only

```json
{ "email": "kit@example.com", "role": "uploader" }
```

```json
{
  "id": 1,
  "email": "kit@example.com",
  "roleName": "uploader",
  "code": "abc12",
  "acceptUrl": "http://localhost:4200/join?code=abc12&email=kit%40example.com",
  "emailSent": false,
  "expiresAt": "2026-09-18T00:00:00Z"
}
```

If `Smtp:Host` is empty, `emailSent` is false and the owner shares the link or code.

### Accept

- **Method:** POST
- **Path:** `/api/invites/accept`
- **Authentication:** Optional session cookie

```json
{ "email": "kit@example.com", "code": "abc12" }
```

Signed-out + matching invite → `401` with `needsLogin` and `loginUrl` (`/api/auth/google/login?invite=`). After Google login, the callback accepts the invite when the Google email matches.
