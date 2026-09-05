# Graph Report - TrackLink  (2026-09-05)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 882 nodes · 1780 edges · 38 communities (35 shown, 3 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 78 edges (avg confidence: 0.84)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `1fd664a1`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Community 0
- Community 1
- Community 2
- Community 3
- Community 4
- Community 5
- Community 6
- Community 7
- Community 8
- Community 9
- Community 10
- Community 11
- Community 12
- Community 13
- Community 14
- Community 15
- Community 16
- Community 17
- Community 18
- Community 19
- Community 20
- Community 21
- Community 22
- Community 23
- Community 24
- Community 25
- Community 26
- Community 27
- Community 28
- Community 29
- Community 30
- Community 31
- Community 32
- Community 33
- Community 34
- Community 35
- Community 36
- Community 37

## God Nodes (most connected - your core abstractions)
1. `DatabaseContext` - 63 edges
2. `StudioPageComponent` - 44 edges
3. `api.Services` - 36 edges
4. `Member` - 34 edges
5. `AlbumActionResult` - 33 edges
6. `api.Data.Entities` - 31 edges
7. `AlbumProposal` - 30 edges
8. `api.Data` - 30 edges
9. `api.Contracts` - 29 edges
10. `AlbumProposalDto` - 27 edges

## Surprising Connections (you probably didn't know these)
- `Handler` --references--> `DatabaseContext`  [EXTRACTED]
  api/Handlers/Albums/AddProposalReview.cs → api/Data/DatabaseContext.cs
- `Handler` --references--> `DatabaseContext`  [EXTRACTED]
  api/Handlers/Albums/CreateAlbum.cs → api/Data/DatabaseContext.cs
- `Handler` --references--> `DatabaseContext`  [EXTRACTED]
  api/Handlers/Albums/CreateProposal.cs → api/Data/DatabaseContext.cs
- `Handler` --references--> `IGoogleDriveService`  [EXTRACTED]
  api/Handlers/Albums/CreateProposal.cs → api/Integrations/Google/IGoogleDriveService.cs
- `Handler` --references--> `DatabaseContext`  [EXTRACTED]
  api/Handlers/Albums/DecideProposal.cs → api/Data/DatabaseContext.cs

## Import Cycles
- None detected.

## Communities (38 total, 3 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.06
Nodes (72): AlbumActionResult, AlbumDetailDto, AlbumProposalDto, AlbumSummaryDto, AlbumTrackDto, ProposalDecisionDto, ProposalReviewDto, WaitingMemberDto (+64 more)

### Community 1 - "Community 1"
Cohesion: 0.05
Nodes (55): AcceptInviteRequest, AddDriveSongRequest, CreateAlbumRequest, CreateBandRequest, CreateInviteRequest, CreateProposalRequest, CreateProposalReviewRequest, PickerTokenDto (+47 more)

### Community 2 - "Community 2"
Cohesion: 0.06
Nodes (45): DriveFileDto, GoogleStatusDto, GetSession, Handler, Query, CancellationToken, Task, DriveListResult (+37 more)

### Community 3 - "Community 3"
Cohesion: 0.13
Nodes (13): AlbumProposalStatuses, api.Services, api.Handlers.Bands, api.Handlers.Invites, api.Handlers.Albums, api.Handlers.Members, api.Handlers.Auth, api (+5 more)

### Community 4 - "Community 4"
Cohesion: 0.07
Nodes (33): BandSummaryDto, MemberDto, MemberSummaryDto, RoleSummaryDto, Guid, Role, CreatedBy, CreatedByNavigation (+25 more)

### Community 5 - "Community 5"
Cohesion: 0.07
Nodes (27): InviteDto, Command, CreateBandInvite, Handler, CancellationToken, IOptions, Task, GoogleOptions (+19 more)

### Community 7 - "Community 7"
Cohesion: 0.08
Nodes (18): Initial, DateTime, Guid, MigrationBuilder, FixRelationsAndGoogleAccount, DateTimeOffset, MigrationBuilder, GoogleMemberSessionAndInvites (+10 more)

### Community 8 - "Community 8"
Cohesion: 0.07
Nodes (8): GoogleFolderPicker, PickedFolder, PickerCallbackData, PickerSurface, readPickerApi(), Injectable, TrackLinkApi, Injectable

### Community 9 - "Community 9"
Cohesion: 0.07
Nodes (25): AlbumTrack, AddedAt, Album, AlbumId, Id, Proposal, ProposalId, Song (+17 more)

### Community 10 - "Community 10"
Cohesion: 0.09
Nodes (22): DatabaseContext, AlbumProposalDecisions, AlbumProposalReviews, AlbumProposals, Albums, AlbumTracks, BandInvites, BandMembers (+14 more)

### Community 11 - "Community 11"
Cohesion: 0.13
Nodes (17): SongDto, AddBandDriveSong, AddDriveSongResult, Command, Handler, CancellationToken, Task, Handler (+9 more)

### Community 12 - "Community 12"
Cohesion: 0.09
Nodes (22): Member, AlbumProposalDecisions, AlbumProposalReviews, AlbumProposals, BandMembers, CreatedAlbums, CreatedInvites, Email (+14 more)

### Community 13 - "Community 13"
Cohesion: 0.13
Nodes (9): AppComponent, Component, appConfig, mergedServerConfig, serverConfig, clientApplicationRoutes, serverRoutes, JoinPageComponent (+1 more)

### Community 14 - "Community 14"
Cohesion: 0.12
Nodes (10): AcceptInviteResultDto, AcceptBandInvite, Command, Handler, CancellationToken, Task, AlbumApproval, AlbumProposalDecisions (+2 more)

### Community 15 - "Community 15"
Cohesion: 0.14
Nodes (7): AlbumDetail, AlbumProposal, ProposalReview, formatReviewMs(), formatReviewRange(), StudioAlbumWorkComponent, Component

### Community 16 - "Community 16"
Cohesion: 0.15
Nodes (10): DriveFile, Member, SessionService, Injectable, StudioDeckComponent, Component, StudioDriveListComponent, Component (+2 more)

### Community 17 - "Community 17"
Cohesion: 0.11
Nodes (17): Band, Albums, BandInvites, BandMembers, CreatedDate, DriveFolderId, DriveFolderName, Genre (+9 more)

### Community 18 - "Community 18"
Cohesion: 0.15
Nodes (12): AudioPlaybackService, CancellationToken, Task, AudioOpenResult, ContentType, EnableRangeProcessing, Stream, IAudioPlaybackService (+4 more)

### Community 19 - "Community 19"
Cohesion: 0.13
Nodes (14): Album, ApprovalRule, Archived, ArtDriveFileId, BandId, CreatedBy, CreatedByNavigation, CreatedDate (+6 more)

### Community 20 - "Community 20"
Cohesion: 0.13
Nodes (14): AlbumProposal, Album, AlbumId, CreatedAt, Decisions, Id, ProposedBy, ProposedByNavigation (+6 more)

### Community 21 - "Community 21"
Cohesion: 0.22
Nodes (10): credentialsInterceptor(), AcceptInviteResult, BandSummary, GoogleStatus, Invite, MemberSummary, PickerToken, ProposalDecision (+2 more)

### Community 22 - "Community 22"
Cohesion: 0.14
Nodes (13): net9.0, Google.Apis.Auth (1.68.0), Google.Apis.Drive.v3 (1.68.0.3601), MediatR (12.3.0), Microsoft.EntityFrameworkCore (8.0.6), Microsoft.EntityFrameworkCore.Abstractions (8.0.6), Microsoft.EntityFrameworkCore.Design (8.0.6), Microsoft.EntityFrameworkCore.Relational (8.0.6) (+5 more)

### Community 23 - "Community 23"
Cohesion: 0.14
Nodes (13): BandInvite, AcceptedAt, Band, BandId, Code, CreatedBy, CreatedByNavigation, CreatedDate (+5 more)

### Community 24 - "Community 24"
Cohesion: 0.19
Nodes (9): Member, MemberRole, Id, Member, MemberId, Role, RoleId, MemberDtoMapper (+1 more)

### Community 25 - "Community 25"
Cohesion: 0.17
Nodes (11): AlbumProposalReview, Body, CreatedAt, EndMs, Id, Member, MemberId, Proposal (+3 more)

### Community 26 - "Community 26"
Cohesion: 0.33
Nodes (6): Command, CompleteGoogleLogin, Handler, CancellationToken, Member, Task

### Community 27 - "Community 27"
Cohesion: 0.20
Nodes (9): GoogleAccount, AccessToken, Email, ExpiresAt, Id, Member, MemberId, RefreshToken (+1 more)

### Community 28 - "Community 28"
Cohesion: 0.22
Nodes (8): Song, Vote, Comment, Id, Member, MemberId, Song, SongId

### Community 29 - "Community 29"
Cohesion: 0.22
Nodes (8): AlbumProposalDecision, Decision, Id, MemberId, Proposal, ProposalId, UpdatedAt, DateTime

### Community 30 - "Community 30"
Cohesion: 0.39
Nodes (6): MemberController, CancellationToken, HttpGet, IActionResult, ISender, Task

### Community 31 - "Community 31"
Cohesion: 0.25
Nodes (7): Band, SongIdentifier, Band, BandId, Id, Song, SongId

### Community 32 - "Community 32"
Cohesion: 0.25
Nodes (7): BandMember, Band, BandId, Id, Member, MemberId, RoleName

### Community 33 - "Community 33"
Cohesion: 0.33
Nodes (3): AlbumSummary, StudioAlbumDeskComponent, Component

### Community 34 - "Community 34"
Cohesion: 0.33
Nodes (5): MigrateDatabaseStartupFilter, Action, IApplicationBuilder, IConfiguration, IStartupFilter

### Community 36 - "Community 36"
Cohesion: 0.33
Nodes (5): angularApp, browserDistFolder, expressApplication, reqHandler, serverDistFolder

## Knowledge Gaps
- **209 isolated node(s):** `CreateBandRequest`, `ProposalDecision`, `RoleSummary`, `WaitingMember`, `AlbumProposalStatuses` (+204 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 376 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `DatabaseContext` connect `Community 10` to `Community 0`, `Community 2`, `Community 3`, `Community 4`, `Community 5`, `Community 9`, `Community 11`, `Community 14`, `Community 18`, `Community 19`, `Community 20`, `Community 23`, `Community 24`, `Community 25`, `Community 26`, `Community 27`, `Community 28`, `Community 29`, `Community 31`, `Community 32`, `Community 34`?**
  _High betweenness centrality (0.220) - this node is a cross-community bridge._
- **Why does `GoogleDriveService` connect `Community 2` to `Community 10`, `Community 3`, `Community 5`?**
  _High betweenness centrality (0.070) - this node is a cross-community bridge._
- **Why does `api.Data` connect `Community 3` to `Community 7`?**
  _High betweenness centrality (0.067) - this node is a cross-community bridge._
- **What connects `CreateBandRequest`, `ProposalDecision`, `RoleSummary` to the rest of the system?**
  _209 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.05613951266125179 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.052604698672114404 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.056338028169014086 - nodes in this community are weakly interconnected._