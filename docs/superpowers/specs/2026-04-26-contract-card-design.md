# Buyer–Seller Contract Card Feature Design

**Date:** 2026-04-26  
**Status:** Approved

---

## Problem & Approach

In Lithuania, a vehicle purchase-sale (pirkimo–pardavimo sutartis) requires a standardised paper form approved by VKTI. Users currently fill this out manually and upload a PDF to Regitra. Since the marketplace already has listing data (make, model, VIN, mileage) and both parties' accounts, we can pre-fill most fields and allow both parties to fill in the rest through a structured form in the chat interface. The result is a generated PDF matching the official form layout.

The feature follows the existing chat card pattern (same as Offer, Meeting, AvailabilityCard): a card in the message thread that tracks a multi-step collaborative workflow between buyer and seller.

---

## Data Model

### New Domain Entities

#### `ContractCard`
```
Id                  Guid
ConversationId      Guid  (FK → Conversation)
InitiatorId         Guid  (FK → User)
Status              ContractCardStatus
AcceptedAt          DateTime?
CreatedAt           DateTime
```
Navigation: `SellerSubmission?`, `BuyerSubmission?`, `Message?`, `Conversation`, `Initiator`

#### `ContractSellerSubmission`
Seller fills this when they click "Fill out". Contains vehicle data (only the seller knows this) plus seller personal info.

```
Id                          Guid
ContractCardId              Guid (FK → ContractCard)

-- Vehicle
SdkCode                     string?
Make                        string
CommercialName              string
RegistrationNumber          string
Mileage                     int
Vin                         string?
RegistrationCertificate     string?
TechnicalInspectionValid    bool
WasDamaged                  bool
DamageKnown                 bool?        (null when WasDamaged = false)
DefectBrakes                bool
DefectSafety                bool
DefectSteering              bool
DefectExhaust               bool
DefectLighting              bool
DefectDetails               string?
Price                       decimal?

-- Seller personal
PersonalIdCode              string
FullName                    string
Phone                       string
Email                       string
Address                     string
Country                     string       (default: "Lietuva")
SubmittedAt                 DateTime
```

#### `ContractBuyerSubmission`
Buyer fills only their personal info.

```
Id                  Guid
ContractCardId      Guid (FK → ContractCard)
PersonalIdCode      string
FullName            string
Phone               string
Email               string
Address             string
SubmittedAt         DateTime
```

### `ContractCardStatus` Enum
```
Pending = 0          Request sent, waiting for response
Active = 1           Accepted, both can fill their forms
SellerSubmitted = 2  Only seller has submitted
BuyerSubmitted = 3   Only buyer has submitted
Complete = 4         Both submitted, PDF available
Declined = 5         Recipient declined the request
Cancelled = 6        Initiator cancelled while Pending
```

### Changes to Existing Entities

**`Message`** — add:
- `ContractCardId Guid?`
- Navigation: `ContractCard?`

**`MessageType` enum** — add:
- `Contract = 4`

**`User`** — add 3 nullable fields for profile pre-fill:
- `PhoneNumber string?`
- `PersonalIdCode string?`
- `Address string?`

---

## Backend Architecture

### CQRS Handlers

| Handler | Layer | Description |
|---|---|---|
| `RequestContractCommand` | Application | Creates `ContractCard` (Pending), creates `Message` (type Contract), broadcasts SignalR. Returns 409 if active contract already exists in conversation. |
| `RespondToContractCommand` | Application | Accept → Active or Decline → Declined. Only recipient can respond. Broadcasts SignalR. |
| `SubmitContractSellerFormCommand` | Application | Saves `ContractSellerSubmission`. Transitions: if BuyerSubmitted → Complete, else → SellerSubmitted. Only seller can call this. Broadcasts SignalR. Optionally updates User profile fields. |
| `SubmitContractBuyerFormCommand` | Application | Saves `ContractBuyerSubmission`. Transitions: if SellerSubmitted → Complete, else → BuyerSubmitted. Only buyer can call this. Broadcasts SignalR. Optionally updates User profile fields. |
| `GetContractCardQuery` | Application | Returns current card state with both submissions' metadata (submitted timestamps, not personal data). Used by frontend on SignalR-triggered refresh. |
| `ExportContractPdfQuery` | Application | Returns PDF bytes. Returns 403 if status ≠ Complete. |
| `UpdateUserContractProfileCommand` | Application | Saves PhoneNumber, PersonalIdCode, Address to User. |
| `GetUserContractProfileQuery` | Application | Returns the three nullable profile fields for form pre-fill. |

### PDF Generation

A `ContractPdfService` in the Infrastructure layer uses **QuestPDF** to generate the official VKTI vehicle purchase-sale form layout. The PDF:
- Matches the official Lithuanian form structure (header, vehicle section, seller/buyer columns)
- Includes all submitted data from both `ContractSellerSubmission` and `ContractBuyerSubmission`
- Is generated on-demand (not stored) — always reflects the submitted data
- Returned as `application/pdf` bytes from `ExportContractPdfQuery`

### SignalR

Reuses the existing hub. Contract card state changes broadcast to both conversation participants using the same pattern as offer/meeting cards. Event payloads: `ContractCardUpdated` carrying the updated card state.

### Authorization

- Only the conversation's buyer (`Conversation.BuyerId`) or seller (`Conversation.Listing.SellerId`) may interact with a contract card
- Seller-only endpoints: `SubmitContractSellerFormCommand`
- Buyer-only endpoints: `SubmitContractBuyerFormCommand`
- Both: `RespondToContractCommand` (recipient only), `ExportContractPdfQuery` (Complete only)

---

## Frontend Architecture

### `ActionBar.tsx` changes
- 4th menu item added to the `+` popover: "Pirkimo–pardavimo sutartis" with a `FileText` icon (Lucide)
- Item is disabled (with tooltip) when a Pending or Active contract already exists in the conversation

### New Components

#### `ContractCard.tsx`
Renders all 6 meaningful states. Uses a `statusConfig` pattern (same as `AvailabilityCardComponent`):

| State | Header colour | What each user sees |
|---|---|---|
| Pending (recipient) | Blue | Accept / Decline buttons |
| Pending (initiator) | Blue | "Waiting for response..." + Cancel button |
| Active | Blue | Submission status badges (Seller: Pending, Buyer: Pending) + "Fill out" button |
| SellerSubmitted / BuyerSubmitted | Blue | One badge green, one yellow. Pending party sees "Fill out" button. Already-submitted party sees "View submitted data" (opens read-only dialog). |
| Complete | Green | Both badges green + "Export PDF" button |
| Declined / Cancelled | Grey | Informational text only |

#### `ContractFormDialog.tsx`
2-step wizard dialog. Once a party has submitted, the same dialog opens in **read-only mode** (all fields locked, no submit button, labelled "Submitted on [date]"). Both parties see the read-only view after Complete status.

**Step 1 — Vehicle details** *(seller only; buyer skips directly to Step 2)*
- Auto-filled (editable): Make, Commercial Name, VIN, Mileage, Price (from accepted offer if available)
- Manual: Registration number, SDK code, Registration certificate series & number
- Checkboxes: Technical inspection valid/invalid
- Checkboxes: Was vehicle damaged (yes/no); if yes — damage known/unknown
- Defect checkboxes (multi-select): Brake system, Safety systems, Steering & suspension, Exhaust system, Lighting system
- Text area: Defect/incident details

**Step 2 — Personal info** *(both seller and buyer)*
- Auto-filled (locked): Email (from account)
- Pre-filled from profile if available (editable): Phone, Personal ID code / company code, Address
- "Remember for next time" checkbox — if checked, updates `UserContractProfile` on submit
- Country field (default: "Lietuva", editable)

### New Frontend Types
- `ContractCard.ts` — mirrors `AvailabilityCard.ts` pattern
- `ContractEventPayloads.ts` — SignalR event payload types

### PDF Export
`ExportContractPdfQuery` is called as a file-download API request (blob response). No client-side PDF library needed.

### i18n
All new strings added to both `lt/chat.json` and `en/chat.json`. Lithuanian text uses official VKTI form field names where applicable (e.g. "Transporto priemonės savininko deklaravimo kodas").

---

## Error Handling & Edge Cases

| Scenario | Handling |
|---|---|
| Duplicate contract request | `RequestContractCommand` returns 409; ActionBar item disabled when Pending/Active exists |
| Concurrent submissions | EF Core transaction ensures atomic status transition to Complete |
| Contract cancellation | Initiator can cancel while Pending only; once Active, the card is locked |
| PDF before Complete | Server returns 403; Export button hidden until both parties submit |
| Form re-submission | Once submitted, party sees read-only "View submitted data" instead of "Fill out" |
| Listing deleted mid-contract | Contract card continues functioning with already-saved data |
| Profile save failure | Non-critical — contract submission succeeds even if profile update fails (separate operation) |

---

## What Gets Auto-Filled

| Field | Source |
|---|---|
| Make | `Listing.Variant.Model.Make.Name` |
| Commercial name | `Listing.Variant.Name` or `Listing.Variant.Model.Name` |
| VIN | `Listing.Vin` |
| Mileage | `Listing.Mileage` |
| Price | Latest accepted `Offer.Amount` in conversation (if present) |
| Seller email | `User.Email` |
| Buyer email | `User.Email` |
| Seller phone/ID/address | `User.PhoneNumber` / `User.PersonalIdCode` / `User.Address` (if saved) |
| Buyer phone/ID/address | Same profile fields |
