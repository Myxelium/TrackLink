# Album

Albums are catalogs, not audio containers. The studio Albums rail lists them, opens one desk, and reviews a proposal like a small pull request.

Play accepted takes and proposed takes through `/api/songs/{id}/audio` only. Bytes stay in Drive or at a URL.

Owner and uploaders create albums and open proposals. Any band member can approve or reject. The album `approvalRule` (`all` or `owner_uploaders`) decides when a take is admitted.

Band members can pin a start/end comment on an open, rejected, or approved proposal. Clicking a note seeks the transport to that start. Withdrawn proposals are read-only.
