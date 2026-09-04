# Identity

Browser session for the **current member**. Persistence is `localStorage` key `tracklink.memberId`, sent as context for later Drive-per-member work. The first page uses it to load `GET /api/members/{id}`.
