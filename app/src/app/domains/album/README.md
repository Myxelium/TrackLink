# Album

Albums are catalogs, not audio containers. The studio Albums rail lists them, opens one desk, and reviews a proposal like a small pull request.

Play accepted takes and proposed takes through `/api/songs/{id}/audio` only. Bytes stay in Drive or at a URL.

Owner and uploaders create albums and open proposals. Any band member can approve or reject. The album `approvalRule` (`all` or `owner_uploaders`) decides when a take is admitted.

Admitted takes also have an inclusion poll (in / out / abstain). That tally does not add or remove a track — proposal approve still owns admission.

Album and song titles have a separate straw poll. Members propose a candidate and cast one vote per contest. Tallies show on the desk; the official album or song name does not change until someone applies a winner later.

Admitted takes also have a ranked order poll. Members submit a complete ranking. The desk shows average rank and consensus position. Owners and uploaders can lock that consensus onto `sortOrder`; further rankings are rejected while locked.

Album art is a separate Drive-file poll. Members vote on a file id that must stay inside the band folder and be an image. Any member can upload a local image into that folder as a candidate; upload does not apply the cover. The desk lists sandboxed images (`kind=image`), shows the API size warning when a side is outside 1600–3000, shows candidate thumbs from `/api/albums/{id}/art?fileId=`, and previews the applied cover from `/api/albums/{id}/art`. A typed non-image id is rejected. Upload errors (including Drive write scope / re-login) show on the album banner and must not white-screen. Owners and uploaders can apply the winner as `artDriveFileId`, freeze further art votes and uploads, and unlock without changing the cover. Track order has the same lock/unlock pair. If Drive no longer has the applied cover, album GET clears it.

Band members can pin a start/end comment on an open, rejected, or approved proposal. Clicking a note seeks the transport to that start. Withdrawn proposals are read-only.
